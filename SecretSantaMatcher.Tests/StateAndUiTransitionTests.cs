using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xunit;
using SecretSantaMatcher.Models;
using SecretSantaMatcher.Services;

namespace SecretSantaMatcher.Tests
{
    public class SessionBackupFixture : IDisposable
    {
        private readonly string _sessionFilePath;
        private readonly string? _originalContent;
        private readonly bool _existed;

        public SessionBackupFixture()
        {
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "SecretSantaMatcher"
            );
            _sessionFilePath = Path.Combine(appDataFolder, "secretsanta_session.json");

            if (File.Exists(_sessionFilePath))
            {
                _existed = true;
                _originalContent = File.ReadAllText(_sessionFilePath);
            }
            else
            {
                _existed = false;
                _originalContent = null;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_existed && _originalContent != null)
                {
                    string? dir = Path.GetDirectoryName(_sessionFilePath);
                    if (dir != null && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllText(_sessionFilePath, _originalContent);
                }
                else
                {
                    if (File.Exists(_sessionFilePath))
                    {
                        File.Delete(_sessionFilePath);
                    }
                }
            }
            catch
            {
                // Suppress errors in test environment cleanup
            }
        }
    }

    public class StateAndUiTransitionTests
    {
        private void RunInSTA(Action action)
        {
            Exception? exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (Application.Current == null)
                    {
                        var app = new App();
                        app.InitializeComponent();
                    }
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }

        private T GetPrivateField<T>(MainWindow window, string fieldName)
        {
            var field = typeof(MainWindow).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field == null)
            {
                var prop = typeof(MainWindow).GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (prop != null)
                {
                    return (T)prop.GetValue(window)!;
                }
                throw new InvalidOperationException($"Could not find field or property '{fieldName}' in MainWindow.");
            }
            return (T)field.GetValue(window)!;
        }

        private void SetMessageBoxShowHandler(MainWindow window, Func<string, string, MessageBoxButton, MessageBoxImage, MessageBoxResult> handler)
        {
            var prop = typeof(MainWindow).GetProperty("MessageBoxShowHandler", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (prop == null)
            {
                throw new InvalidOperationException("Could not find property 'MessageBoxShowHandler' in MainWindow.");
            }
            prop.SetValue(window, handler);
        }

        private void InvokePrivateMethod(MainWindow window, string methodName, params object[] args)
        {
            var method = typeof(MainWindow).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
            {
                throw new InvalidOperationException($"Could not find private method '{methodName}' in MainWindow.");
            }
            method.Invoke(window, args);
        }

        [Fact]
        public void EditMode_EnterCancelSave_ShouldUpdateListBoxCleanly()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // 1. Arrange: Create MainWindow
                var window = new MainWindow();
                
                var inputName = GetPrivateField<TextBox>(window, "InputName");
                var inputEmail = GetPrivateField<TextBox>(window, "InputEmail");
                var inputWishlist = GetPrivateField<TextBox>(window, "InputWishlist");
                var submitParticipantBtn = GetPrivateField<Button>(window, "SubmitParticipantBtn");
                var cancelEditBtn = GetPrivateField<Button>(window, "CancelEditBtn");
                var participantsList = GetPrivateField<ItemsControl>(window, "ParticipantsList");
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");

                // Add a participant by filling the form and clicking save
                inputName.Text = "Alice";
                inputEmail.Text = "alice@example.com";
                inputWishlist.Text = "http://alice-wishlist";
                
                var addBtn = new Button();
                InvokePrivateMethod(window, "AddParticipant_Click", addBtn, new RoutedEventArgs());

                // Verify participant is added and binds to UI ListBox cleanly
                Assert.Single(participants);
                var alice = participants.First();
                Assert.Equal("Alice", alice.Name);
                Assert.Equal("alice@example.com", alice.Email);
                Assert.Same(participants, participantsList.ItemsSource);

                // 2. Act: Trigger Edit Mode
                var editBtn = new Button { Tag = alice.Id };
                InvokePrivateMethod(window, "EditParticipant_Click", editBtn, new RoutedEventArgs());

                // Assert we entered edit mode correctly
                Assert.Equal("Save Changes", submitParticipantBtn.Content);
                Assert.Equal(Visibility.Visible, cancelEditBtn.Visibility);
                Assert.Equal("Alice", inputName.Text);

                // 3. Act: Cancel Edit Mode
                var cancelBtn = new Button();
                InvokePrivateMethod(window, "CancelEdit_Click", cancelBtn, new RoutedEventArgs());

                // Assert Edit mode is closed and inputs cleared
                Assert.Equal("Add Exchange Member", submitParticipantBtn.Content);
                Assert.Equal(Visibility.Collapsed, cancelEditBtn.Visibility);
                Assert.Equal(string.Empty, inputName.Text);

                // 4. Act: Edit again and save changes
                InvokePrivateMethod(window, "EditParticipant_Click", editBtn, new RoutedEventArgs());
                inputName.Text = "Alice Edited";
                inputEmail.Text = "alice.edited@example.com";
                
                InvokePrivateMethod(window, "AddParticipant_Click", addBtn, new RoutedEventArgs());

                // Assert changes are applied and edit mode ended
                Assert.Equal("Add Exchange Member", submitParticipantBtn.Content);
                Assert.Equal(Visibility.Collapsed, cancelEditBtn.Visibility);
                Assert.Equal(string.Empty, inputName.Text);

                // Verify list box contains updated information cleanly
                Assert.Single(participants);
                Assert.Equal("Alice Edited", participants[0].Name);
                Assert.Equal("alice.edited@example.com", participants[0].Email);
            });
        }

        [Fact]
        public void DeleteParticipant_ShouldCleanUpBidirectionalSignificantOtherReferencesImmediately()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                var window = new MainWindow();

                var inputName = GetPrivateField<TextBox>(window, "InputName");
                var inputEmail = GetPrivateField<TextBox>(window, "InputEmail");
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");

                // 1. Arrange: Add Alice and Bob
                inputName.Text = "Alice";
                inputEmail.Text = "alice@example.com";
                InvokePrivateMethod(window, "AddParticipant_Click", new Button(), new RoutedEventArgs());

                inputName.Text = "Bob";
                inputEmail.Text = "bob@example.com";
                InvokePrivateMethod(window, "AddParticipant_Click", new Button(), new RoutedEventArgs());

                Assert.Equal(2, participants.Count);

                var alice = participants.First(x => x.Name == "Alice");
                var bob = participants.First(x => x.Name == "Bob");

                // Establish bidirectional exclusions/significant other references
                alice.ExcludedParticipantIds.Add(bob.Id);
                alice.SignificantOtherId = bob.Id;

                bob.ExcludedParticipantIds.Add(alice.Id);
                bob.SignificantOtherId = alice.Id;

                // 2. Act: Delete Bob with confirmation
                SetMessageBoxShowHandler(window, (msg, title, buttons, icon) => MessageBoxResult.Yes);
                var deleteBtn = new Button { Tag = bob.Id };
                InvokePrivateMethod(window, "DeleteParticipant_Click", deleteBtn, new RoutedEventArgs());

                // 3. Assert: Verify Bob is removed and Alice's references are cleaned up immediately
                Assert.Single(participants);
                Assert.DoesNotContain(participants, x => x.Id == bob.Id);

                Assert.DoesNotContain(bob.Id, alice.ExcludedParticipantIds);
                Assert.NotEqual(bob.Id, alice.SignificantOtherId);
                Assert.Equal(string.Empty, alice.SignificantOtherId);
            });
        }

        [Fact]
        public void DeleteParticipant_WithConfirmationYes_ShouldDeleteParticipant()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                var inputName = GetPrivateField<TextBox>(window, "InputName");
                var inputEmail = GetPrivateField<TextBox>(window, "InputEmail");
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");

                inputName.Text = "Charlie";
                inputEmail.Text = "charlie@example.com";
                InvokePrivateMethod(window, "AddParticipant_Click", new Button(), new RoutedEventArgs());

                Assert.Single(participants);
                var charlie = participants.First();

                SetMessageBoxShowHandler(window, (msg, title, buttons, icon) => MessageBoxResult.Yes);
                var deleteBtn = new Button { Tag = charlie.Id };

                // Act
                InvokePrivateMethod(window, "DeleteParticipant_Click", deleteBtn, new RoutedEventArgs());

                // Assert
                Assert.Empty(participants);
            });
        }

        [Fact]
        public void DeleteParticipant_WithConfirmationNo_ShouldNotDeleteParticipant()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                var inputName = GetPrivateField<TextBox>(window, "InputName");
                var inputEmail = GetPrivateField<TextBox>(window, "InputEmail");
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");

                inputName.Text = "Charlie";
                inputEmail.Text = "charlie@example.com";
                InvokePrivateMethod(window, "AddParticipant_Click", new Button(), new RoutedEventArgs());

                Assert.Single(participants);
                var charlie = participants.First();

                SetMessageBoxShowHandler(window, (msg, title, buttons, icon) => MessageBoxResult.No);
                var deleteBtn = new Button { Tag = charlie.Id };

                // Act
                InvokePrivateMethod(window, "DeleteParticipant_Click", deleteBtn, new RoutedEventArgs());

                // Assert
                Assert.Single(participants);
                Assert.Contains(participants, x => x.Id == charlie.Id);
            });
        }

        [Fact]
        public void ClearAllParticipants_WithConfirmationYes_ShouldClearAllParticipants()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                var inputName = GetPrivateField<TextBox>(window, "InputName");
                var inputEmail = GetPrivateField<TextBox>(window, "InputEmail");
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");

                inputName.Text = "Alice";
                inputEmail.Text = "alice@example.com";
                InvokePrivateMethod(window, "AddParticipant_Click", new Button(), new RoutedEventArgs());

                inputName.Text = "Bob";
                inputEmail.Text = "bob@example.com";
                InvokePrivateMethod(window, "AddParticipant_Click", new Button(), new RoutedEventArgs());

                Assert.Equal(2, participants.Count);

                SetMessageBoxShowHandler(window, (msg, title, buttons, icon) => MessageBoxResult.Yes);

                // Act
                InvokePrivateMethod(window, "ClearAllParticipants_Click", new Button(), new RoutedEventArgs());

                // Assert
                Assert.Empty(participants);
            });
        }

        [Fact]
        public void ClearAllParticipants_WithConfirmationNo_ShouldNotClearAllParticipants()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                var inputName = GetPrivateField<TextBox>(window, "InputName");
                var inputEmail = GetPrivateField<TextBox>(window, "InputEmail");
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");

                inputName.Text = "Alice";
                inputEmail.Text = "alice@example.com";
                InvokePrivateMethod(window, "AddParticipant_Click", new Button(), new RoutedEventArgs());

                inputName.Text = "Bob";
                inputEmail.Text = "bob@example.com";
                InvokePrivateMethod(window, "AddParticipant_Click", new Button(), new RoutedEventArgs());

                Assert.Equal(2, participants.Count);

                SetMessageBoxShowHandler(window, (msg, title, buttons, icon) => MessageBoxResult.No);

                // Act
                InvokePrivateMethod(window, "ClearAllParticipants_Click", new Button(), new RoutedEventArgs());

                // Assert
                Assert.Equal(2, participants.Count);
            });
        }

        [Fact]
        public void ImportSession_WithInvalidOrEmptyJson_ShouldFailGracefully()
        {
            // Arrange
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            
            string nonExistentFile = Path.Combine(tempDir, "doesnotexist.json");
            string emptyFile = Path.Combine(tempDir, "empty.json");
            string malformedFile = Path.Combine(tempDir, "malformed.json");
            string nullDataFile = Path.Combine(tempDir, "null.json");

            File.WriteAllText(emptyFile, "");
            File.WriteAllText(malformedFile, "{ invalid json }");
            File.WriteAllText(nullDataFile, "null");

            try
            {
                // Act & Assert 1: Non-existent file
                var exFileNotFound = Assert.Throws<FileNotFoundException>(() => SessionManager.ImportSession(nonExistentFile));
                Assert.Contains("Selected session file could not be found.", exFileNotFound.Message);

                // Act & Assert 2: Empty file
                Assert.ThrowsAny<Exception>(() => SessionManager.ImportSession(emptyFile));

                // Act & Assert 3: Malformed JSON file
                Assert.Throws<JsonException>(() => SessionManager.ImportSession(malformedFile));

                // Act & Assert 4: Null data file
                var exInvalidData = Assert.Throws<InvalidDataException>(() => SessionManager.ImportSession(nullDataFile));
                Assert.Contains("Invalid file format. Could not load session data.", exInvalidData.Message);
            }
            finally
            {
                // Cleanup
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }

        [Fact]
        public void AddExclusionFromInput_WithValidTypedName_AddsExclusionAndClearsInputs()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");
                var formExclusions = GetPrivateField<ObservableCollection<string>>(window, "_formExclusions");
                var inputSO = GetPrivateField<ComboBox>(window, "InputSO");

                var bob = new Participant { Name = "Bob", Email = "bob@example.com" };
                var charlie = new Participant { Name = "Charlie", Email = "charlie@example.com" };
                
                participants.Add(bob);
                participants.Add(charlie);

                // Populate InputSO.ItemsSource by calling RefreshParticipantsList
                InvokePrivateMethod(window, "RefreshParticipantsList");

                // Simulate typing "bob" (case-insensitive) exactly into the ComboBox Text
                inputSO.SelectedIndex = -1;
                inputSO.Text = "bob";

                // Act
                InvokePrivateMethod(window, "AddExclusionFromInput");

                // Assert
                Assert.Contains(bob.Id, formExclusions);
                Assert.DoesNotContain(charlie.Id, formExclusions);
                Assert.Equal(-1, inputSO.SelectedIndex);
                Assert.Equal(string.Empty, inputSO.Text);
            });
        }

        [Fact]
        public void AddExclusionFromInput_WithPartialTypedName_FallsBackToFirstMatchAndClearsInputs()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");
                var formExclusions = GetPrivateField<ObservableCollection<string>>(window, "_formExclusions");
                var inputSO = GetPrivateField<ComboBox>(window, "InputSO");

                var bob = new Participant { Name = "Bob", Email = "bob@example.com" };
                var charlie = new Participant { Name = "Charlie", Email = "charlie@example.com" };
                
                participants.Add(bob);
                participants.Add(charlie);

                // Populate InputSO.ItemsSource
                InvokePrivateMethod(window, "RefreshParticipantsList");

                // Simulate typing "Ch" (partial/prefix) into the ComboBox Text
                inputSO.SelectedIndex = -1;
                inputSO.Text = "Ch";

                // Act
                InvokePrivateMethod(window, "AddExclusionFromInput");

                // Assert
                Assert.Contains(charlie.Id, formExclusions);
                Assert.DoesNotContain(bob.Id, formExclusions);
                Assert.Equal(-1, inputSO.SelectedIndex);
                Assert.Equal(string.Empty, inputSO.Text);
            });
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetKeyboardState(byte[] lpKeyState);

        private void SimulateKeyDown(Key key, bool isDown)
        {
            byte[] keyState = new byte[256];
            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            keyState[virtualKey] = isDown ? (byte)0x80 : (byte)0;
            SetKeyboardState(keyState);
        }

        [Fact]
        public void InputSO_TextChanged_WhenNavigationKeyDown_BypassesSelectionResetting()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                window.Show(); // Ensure template is applied and controls are focusable

                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");
                var inputSO = GetPrivateField<ComboBox>(window, "InputSO");

                var charlie = new Participant { Name = "Charlie", Email = "charlie@example.com" };
                participants.Add(charlie);

                // Populate InputSO ItemsSource
                InvokePrivateMethod(window, "RefreshParticipantsList");

                // Ensure the ComboBox has keyboard focus so it processes the text changed handler
                inputSO.Focus();

                // 1. Verification WITHOUT Navigation Key (Control case):
                // Select Charlie in the ComboBox, simulate TextChanged, and verify it resets SelectedIndex to -1.
                inputSO.SelectedItem = charlie;
                var expectedIndex = inputSO.SelectedIndex;
                Assert.NotEqual(-1, expectedIndex);

                inputSO.Text = "Char";
                InvokePrivateMethod(window, "InputSO_TextChanged", inputSO, new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None));

                Assert.Equal(-1, inputSO.SelectedIndex);

                // 2. Verification WITH Navigation Key (Test case):
                // Reset the candidate list, re-select Charlie, simulate Key.Down as pressed, and verify SelectedIndex is NOT reset.
                InvokePrivateMethod(window, "RefreshParticipantsList");
                inputSO.SelectedItem = charlie;
                expectedIndex = inputSO.SelectedIndex;
                Assert.NotEqual(-1, expectedIndex);

                SimulateKeyDown(Key.Down, true);
                try
                {
                    inputSO.Text = "Char";
                    InvokePrivateMethod(window, "InputSO_TextChanged", inputSO, new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None));

                    // Assert: SelectedIndex must NOT be reset to -1 because Key.Down was pressed
                    Assert.Equal(expectedIndex, inputSO.SelectedIndex);
                    Assert.Equal(charlie, inputSO.SelectedItem);
                }
                finally
                {
                    // Clean up the keyboard state so it doesn't affect other tests
                    SimulateKeyDown(Key.Down, false);
                }
            });
        }

        [Fact]
        public void InputSO_DropDownClosed_WhenSelectedIndexNotMinusOne_InvokesAddExclusionFromInput()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");
                var formExclusions = GetPrivateField<ObservableCollection<string>>(window, "_formExclusions");
                var inputSO = GetPrivateField<ComboBox>(window, "InputSO");

                var bob = new Participant { Name = "Bob", Email = "bob@example.com" };
                participants.Add(bob);

                // Populate InputSO.ItemsSource
                InvokePrivateMethod(window, "RefreshParticipantsList");

                // Set selection to Bob
                inputSO.SelectedItem = bob;
                Assert.NotEqual(-1, inputSO.SelectedIndex);

                // Act - call the event handler
                InvokePrivateMethod(window, "InputSO_DropDownClosed", inputSO, EventArgs.Empty);

                // Assert: Bob's ID is added to the exclusions list
                Assert.Contains(bob.Id, formExclusions);
                Assert.Equal(-1, inputSO.SelectedIndex);
                Assert.Equal(string.Empty, inputSO.Text);
            });
        }

        [Fact]
        public void InputSO_DropDownClosed_WhenSelectedIndexIsMinusOne_DoesNotInvokeAddExclusionFromInput()
        {
            using var backup = new SessionBackupFixture();
            RunInSTA(() =>
            {
                // Arrange
                var window = new MainWindow();
                var participants = GetPrivateField<ObservableCollection<Participant>>(window, "_participants");
                var formExclusions = GetPrivateField<ObservableCollection<string>>(window, "_formExclusions");
                var inputSO = GetPrivateField<ComboBox>(window, "InputSO");

                var bob = new Participant { Name = "Bob", Email = "bob@example.com" };
                participants.Add(bob);

                // Populate InputSO.ItemsSource
                InvokePrivateMethod(window, "RefreshParticipantsList");

                // Ensure SelectedIndex is -1
                inputSO.SelectedIndex = -1;

                // Act - call the event handler
                InvokePrivateMethod(window, "InputSO_DropDownClosed", inputSO, EventArgs.Empty);

                // Assert: Bob's ID is NOT added to the exclusions list, since it was bypassed
                Assert.Empty(formExclusions);
            });
        }
    }
}
