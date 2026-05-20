using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using SecretSantaMatcher.Models;
using SecretSantaMatcher.Services;

namespace SecretSantaMatcher
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Participant> _participants = new();
        private MatchingResult? _currentMatchingResult;
        private bool _isPasswordRevealed = false;
        private string? _editingParticipantId = null;

        public MainWindow()
        {
            InitializeComponent();
            
            // Register events
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set initial active tab styling
            UpdateTabButtonStyles(TabParticipantsBtn);

            // Load saved session
            LoadSavedSession();

            // Render live preview initially
            UpdateEmailPreview();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Auto-save session on close
            AutoSaveCurrentSession();
        }

        // ==================== SESSION DATA PERSISTENCE ====================

        private void LoadSavedSession()
        {
            try
            {
                var session = SessionManager.LoadSession();
                
                _participants.Clear();
                foreach (var p in session.Participants)
                {
                    _participants.Add(p);
                }

                InputOrganizerName.Text = session.OrganizerName;
                InputSmtpEmail.Text = session.SenderEmail;
                
                if (session.RememberPassword && !string.IsNullOrEmpty(session.SavedPassword))
                {
                    InputSmtpPassword.Password = session.SavedPassword;
                    CheckboxRememberPass.IsChecked = true;
                }
                else
                {
                    InputSmtpPassword.Password = string.Empty;
                    CheckboxRememberPass.IsChecked = false;
                }

                TemplateSubject.Text = session.EmailSubject;
                TemplateBody.Text = session.EmailBody;

                RefreshParticipantsList();
                UpdateStatusBoard();
                Log("Session loaded automatically.");
            }
            catch (Exception ex)
            {
                Log($"[Error] Could not load saved session: {ex.Message}");
            }
        }

        private void AutoSaveCurrentSession()
        {
            try
            {
                var data = GetCurrentSessionData();
                SessionManager.SaveSession(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-save failed: {ex.Message}");
            }
        }

        private SessionData GetCurrentSessionData()
        {
            return new SessionData
            {
                Participants = _participants.ToList(),
                OrganizerName = InputOrganizerName.Text,
                SenderEmail = InputSmtpEmail.Text,
                EmailSubject = TemplateSubject.Text,
                EmailBody = TemplateBody.Text,
                RememberPassword = CheckboxRememberPass.IsChecked == true,
                SavedPassword = _isPasswordRevealed ? InputSmtpPasswordVisible.Text : InputSmtpPassword.Password
            };
        }

        private void RefreshParticipantsList()
        {
            // 1. Rebuild the visual register of names for XAML bindings
            Converters.ParticipantNameRegister.Clear();
            foreach (var p in _participants)
            {
                Converters.ParticipantNameRegister[p.Id] = p.Name;
            }

            // 2. Refresh the visual list
            ParticipantsList.ItemsSource = null;
            ParticipantsList.ItemsSource = _participants;

            // 3. Update the dropdown for Significant Other selection
            var currentSelection = InputSO.SelectedValue;
            var soCandidates = _participants
                .Where(p => string.IsNullOrEmpty(_editingParticipantId) || p.Id != _editingParticipantId)
                .ToList();
            
            InputSO.ItemsSource = null;
            InputSO.ItemsSource = soCandidates;
            InputSO.SelectedValue = currentSelection;

            // 4. Update empty state visibility
            EmptyStatePanel.Visibility = _participants.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CountLabel.Text = $"All Members ({_participants.Count})";

            UpdateStatusBoard();
        }

        private void UpdateStatusBoard()
        {
            StatusTotalGivers.Text = $"{_participants.Count} Members";
            
            if (_currentMatchingResult != null && _currentMatchingResult.Success)
            {
                StatusSolverState.Text = "Ready (Solved)";
                StatusSolverState.Foreground = (Brush)Application.Current.Resources["SuccessBrush"];
                SendEmailsBtn.IsEnabled = true;
            }
            else
            {
                StatusSolverState.Text = "Not Solved";
                StatusSolverState.Foreground = (Brush)Application.Current.Resources["TextMuted"];
                SendEmailsBtn.IsEnabled = false;
            }

            bool smtpConfigured = !string.IsNullOrWhiteSpace(InputSmtpEmail.Text) && 
                                  !string.IsNullOrWhiteSpace(_isPasswordRevealed ? InputSmtpPasswordVisible.Text : InputSmtpPassword.Password);

            if (smtpConfigured)
            {
                StatusSmtpState.Text = "Credentials Entered";
                StatusSmtpState.Foreground = (Brush)Application.Current.Resources["SuccessBrush"];
            }
            else
            {
                StatusSmtpState.Text = "Unconfigured";
                StatusSmtpState.Foreground = (Brush)Application.Current.Resources["ErrorBrush"];
                SendEmailsBtn.IsEnabled = false; // Disable email sending if SMTP credentials missing
            }
        }

        // ==================== TABS NAVIGATION MECHANIC ====================

        private void SwitchToTab(Grid targetPanel, Button activeBtn)
        {
            // Collapse all panels
            TabParticipantsPanel.Visibility = Visibility.Collapsed;
            TabEmailPanel.Visibility = Visibility.Collapsed;
            TabSmtpPanel.Visibility = Visibility.Collapsed;
            TabMatchPanel.Visibility = Visibility.Collapsed;

            // Show selected panel
            targetPanel.Visibility = Visibility.Visible;

            // Highlight selected button style
            UpdateTabButtonStyles(activeBtn);

            // Auto-save session when moving between tabs
            AutoSaveCurrentSession();

            // Specific tab entry actions
            if (targetPanel == TabMatchPanel)
            {
                UpdateStatusBoard();
            }
            else if (targetPanel == TabEmailPanel)
            {
                UpdateEmailPreview();
            }
        }

        private void UpdateTabButtonStyles(Button activeBtn)
        {
            // Set all nav button backgrounds to transparent
            TabParticipantsBtn.Background = Brushes.Transparent;
            TabParticipantsBtn.BorderBrush = Brushes.Transparent;
            TabEmailBtn.Background = Brushes.Transparent;
            TabEmailBtn.BorderBrush = Brushes.Transparent;
            TabSmtpBtn.Background = Brushes.Transparent;
            TabSmtpBtn.BorderBrush = Brushes.Transparent;
            TabMatchBtn.Background = Brushes.Transparent;
            TabMatchBtn.BorderBrush = Brushes.Transparent;

            // Apply selected styling to the active button
            activeBtn.Background = (Brush)Application.Current.Resources["BgCard"];
            activeBtn.BorderBrush = (Brush)Application.Current.Resources["BorderDark"];
        }

        private void TabParticipants_Click(object sender, RoutedEventArgs e) => SwitchToTab(TabParticipantsPanel, TabParticipantsBtn);
        private void TabEmail_Click(object sender, RoutedEventArgs e) => SwitchToTab(TabEmailPanel, TabEmailBtn);
        private void TabSmtp_Click(object sender, RoutedEventArgs e) => SwitchToTab(TabSmtpPanel, TabSmtpBtn);
        private void TabMatch_Click(object sender, RoutedEventArgs e) => SwitchToTab(TabMatchPanel, TabMatchBtn);

        // ==================== TAB: PARTICIPANTS LOGIC ====================

        private void AddParticipant_Click(object sender, RoutedEventArgs e)
        {
            string name = InputName.Text.Trim();
            string email = InputEmail.Text.Trim();
            string wishlist = InputWishlist.Text.Trim();
            string soId = InputSO.SelectedValue as string ?? string.Empty;

            // Basic Form Validation
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Participant's Name is required.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(wishlist) && !wishlist.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !wishlist.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // Auto-prepend https:// if URL was input without it for standard links
                wishlist = "https://" + wishlist;
            }

            // Check if email already registered to prevent duplicates (excluding the current participant being edited)
            if (_participants.Any(p => (string.IsNullOrEmpty(_editingParticipantId) || p.Id != _editingParticipantId) && p.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("This email address has already been added.", "Duplicate Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(_editingParticipantId))
            {
                // EDIT MODE
                var target = _participants.FirstOrDefault(p => p.Id == _editingParticipantId);
                if (target != null)
                {
                    target.Name = name;
                    target.Email = email;
                    target.WishlistUrl = wishlist;
                    
                    // Centralized bidirectional significant other update
                    UpdateSignificantOtherPairing(target.Id, soId);

                    Log($"Updated participant: {name}");
                }

                // Reset Edit Mode State
                _editingParticipantId = null;
                SubmitParticipantBtn.Content = "Add Exchange Member";
                CancelEditBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                // ADD MODE
                var newParticipant = new Participant
                {
                    Name = name,
                    Email = email,
                    WishlistUrl = wishlist,
                    SignificantOtherId = string.Empty // Will be set mutually by UpdateSignificantOtherPairing
                };

                _participants.Add(newParticipant);

                if (!string.IsNullOrEmpty(soId))
                {
                    UpdateSignificantOtherPairing(newParticipant.Id, soId);
                }

                Log($"Added participant: {name}");
            }

            // Reset inputs
            InputName.Text = string.Empty;
            InputEmail.Text = string.Empty;
            InputWishlist.Text = string.Empty;
            InputSO.SelectedIndex = -1;

            // Invalidate current match
            _currentMatchingResult = null;

            RefreshParticipantsList();
            AutoSaveCurrentSession();
        }

        private void DeleteParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                var target = _participants.FirstOrDefault(p => p.Id == id);
                if (target != null)
                {
                    // If the participant being deleted is currently being edited, cancel edit mode
                    if (_editingParticipantId == target.Id)
                    {
                        _editingParticipantId = null;
                        SubmitParticipantBtn.Content = "Add Exchange Member";
                        CancelEditBtn.Visibility = Visibility.Collapsed;
                    }

                    // Centralized cleanup of bidirectional significant other associations
                    UpdateSignificantOtherPairing(target.Id, string.Empty);

                    _participants.Remove(target);
                    _currentMatchingResult = null; // Invalidate match

                    RefreshParticipantsList();
                    AutoSaveCurrentSession();
                    Log($"Removed participant: {target.Name}");
                }
            }
        }

        private void ClearAllParticipants_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to delete ALL participants?", "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                _editingParticipantId = null;
                SubmitParticipantBtn.Content = "Add Exchange Member";
                CancelEditBtn.Visibility = Visibility.Collapsed;

                _participants.Clear();
                _currentMatchingResult = null;
                RefreshParticipantsList();
                AutoSaveCurrentSession();
                Log("Cleared all participants.");
            }
        }

        // ==================== SIGNIFICANT OTHER EXCLUSIONS PAIRING ENGINE ====================

        private void UpdateSignificantOtherPairing(string participantId, string newSoId)
        {
            var participant = _participants.FirstOrDefault(p => p.Id == participantId);
            if (participant == null) return;

            string oldSoId = participant.SignificantOtherId;

            // 1. If the SO didn't change, there's nothing to do
            if (oldSoId == newSoId) return;

            // 2. Clear old SO's link pointing back to participant
            if (!string.IsNullOrEmpty(oldSoId))
            {
                var oldSo = _participants.FirstOrDefault(p => p.Id == oldSoId);
                if (oldSo != null && oldSo.SignificantOtherId == participantId)
                {
                    oldSo.SignificantOtherId = string.Empty;
                }
            }

            // 3. Clear any other participants who might have had participantId as their SO
            foreach (var p in _participants)
            {
                if (p.Id != participantId && p.SignificantOtherId == participantId)
                {
                    p.SignificantOtherId = string.Empty;
                }
            }

            // 4. Set the new SO on the participant
            participant.SignificantOtherId = newSoId;

            // 5. Set mutual link on the new SO pointing back to participant
            if (!string.IsNullOrEmpty(newSoId))
            {
                var newSo = _participants.FirstOrDefault(p => p.Id == newSoId);
                if (newSo != null)
                {
                    // If newSo was already linked to someone else, clear that partner's link first
                    string brandNewSoOldPartnerId = newSo.SignificantOtherId;
                    if (!string.IsNullOrEmpty(brandNewSoOldPartnerId) && brandNewSoOldPartnerId != participantId)
                    {
                        var oldPartnerOfNewSo = _participants.FirstOrDefault(p => p.Id == brandNewSoOldPartnerId);
                        if (oldPartnerOfNewSo != null)
                        {
                            oldPartnerOfNewSo.SignificantOtherId = string.Empty;
                        }
                    }

                    newSo.SignificantOtherId = participantId;
                }
            }
        }

        private void EditParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                var target = _participants.FirstOrDefault(p => p.Id == id);
                if (target != null)
                {
                    _editingParticipantId = target.Id;
                    
                    // Populate form
                    InputName.Text = target.Name;
                    InputEmail.Text = target.Email;
                    InputWishlist.Text = target.WishlistUrl;
                    
                    // Refresh dropdown candidates to exclude editing participant
                    RefreshParticipantsList();
                    
                    InputSO.SelectedValue = target.SignificantOtherId;

                    // Toggle form buttons to Edit mode
                    SubmitParticipantBtn.Content = "Save Changes";
                    CancelEditBtn.Visibility = Visibility.Visible;
                    
                    Log($"Editing participant: {target.Name}");
                }
            }
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            _editingParticipantId = null;
            
            // Clear form inputs
            InputName.Text = string.Empty;
            InputEmail.Text = string.Empty;
            InputWishlist.Text = string.Empty;
            
            RefreshParticipantsList();
            
            InputSO.SelectedIndex = -1;

            // Restore form buttons
            SubmitParticipantBtn.Content = "Add Exchange Member";
            CancelEditBtn.Visibility = Visibility.Collapsed;
            
            Log("Cancelled editing.");
        }

        // ==================== TAB: EMAIL TEMPLATE LOGIC ====================

        private void Template_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEmailPreview();
        }

        private void UpdateEmailPreview()
        {
            if (PreviewSubject == null || PreviewBody == null) return;

            string subjectTmpl = TemplateSubject.Text;
            string bodyTmpl = TemplateBody.Text;
            string organizer = string.IsNullOrWhiteSpace(InputOrganizerName.Text) ? "Santa Organizer" : InputOrganizerName.Text;

            // Make beautiful rendered placeholders in live sandbox
            PreviewSubject.Text = "Subject: " + EmailSender.ReplaceTokens(subjectTmpl, organizer, "Jane Giver", "John Doe", "https://amazon.com/wishlist/johndoe");
            PreviewBody.Text = EmailSender.ReplaceTokens(bodyTmpl, organizer, "Jane Giver", "John Doe", "https://amazon.com/wishlist/johndoe");
        }

        // ==================== TAB: GMAIL CONFIG LOGIC ====================

        private void RevealPassword_Click(object sender, RoutedEventArgs e)
        {
            if (_isPasswordRevealed)
            {
                // Hide password
                InputSmtpPassword.Password = InputSmtpPasswordVisible.Text;
                InputSmtpPasswordVisible.Visibility = Visibility.Collapsed;
                InputSmtpPassword.Visibility = Visibility.Visible;
                RevealPasswordBtn.Content = "👁️";
                _isPasswordRevealed = false;
            }
            else
            {
                // Reveal password
                InputSmtpPasswordVisible.Text = InputSmtpPassword.Password;
                InputSmtpPassword.Visibility = Visibility.Collapsed;
                InputSmtpPasswordVisible.Visibility = Visibility.Visible;
                RevealPasswordBtn.Content = "🔒";
                _isPasswordRevealed = true;
            }
            UpdateStatusBoard();
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            string email = InputSmtpEmail.Text.Trim();
            string password = _isPasswordRevealed ? InputSmtpPasswordVisible.Text : InputSmtpPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both your Gmail Address and Gmail App Password before testing.", "Credential Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SmtpStatusText.Text = "Testing Connection...";
            SmtpStatusText.Foreground = (Brush)Application.Current.Resources["WarningBrush"];
            SmtpStatusBadge.BorderBrush = (Brush)Application.Current.Resources["WarningBrush"];
            Log("Initiating Gmail SMTP connection credentials test...");

            try
            {
                await EmailSender.TestConnectionAsync(email, password);

                SmtpStatusText.Text = "Connected!";
                SmtpStatusText.Foreground = (Brush)Application.Current.Resources["SuccessBrush"];
                SmtpStatusBadge.BorderBrush = (Brush)Application.Current.Resources["SuccessBrush"];
                
                Log("[Success] Gmail SMTP connection established successfully! App Password verified.");
                MessageBox.Show("Gmail SMTP Connection Succeeded! A test email was sent to your own inbox.", "SMTP Connection Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                UpdateStatusBoard();
                AutoSaveCurrentSession();
            }
            catch (Exception ex)
            {
                SmtpStatusText.Text = "Failed";
                SmtpStatusText.Foreground = (Brush)Application.Current.Resources["ErrorBrush"];
                SmtpStatusBadge.BorderBrush = (Brush)Application.Current.Resources["ErrorBrush"];

                Log($"[Error] Gmail SMTP connection failed:\n{ex.Message}");
                MessageBox.Show($"Connection failed!\n\nDetails: {ex.Message}\n\nMake sure your Gmail Address is correct and you are using a 16-character App Password, NOT your regular account password.", "SMTP Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== TAB: SOLVER & EMAILING LOGIC ====================

        private void GenerateMatches_Click(object sender, RoutedEventArgs e)
        {
            Log("Running Backtracking Secret Santa matching solver...");

            if (_participants.Count < 2)
            {
                MessageBox.Show("You must add at least 2 participants to generate a secret exchange circle.", "Exchanger Shortage", MessageBoxButton.OK, MessageBoxImage.Warning);
                Log("[Failed] Matcher aborted. Need at least 2 participants.");
                return;
            }

            var result = MatchingSolver.GenerateMatches(_participants.ToList());
            _currentMatchingResult = result;

            if (result.Success)
            {
                Log($"[Success] Computed valid Secret Santa circle for {_participants.Count} participants! Perfect matching loop verified under exclusions.");
                
                // Load matches into presentation box
                var matchPresentations = new List<object>();
                foreach (var match in result.Matches)
                {
                    string giverName = _participants.FirstOrDefault(p => p.Id == match.Key)?.Name ?? "Unknown";
                    string receiverName = _participants.FirstOrDefault(p => p.Id == match.Value)?.Name ?? "Unknown";
                    matchPresentations.Add(new { GiverName = giverName, ReceiverName = receiverName });
                }
                MatchesListBox.ItemsSource = matchPresentations;

                UpdateStatusBoard();
            }
            else
            {
                Log($"[Failed] Backtracking Solver bottleneck. Constraints unsolvable:\n{result.ErrorMessage}");
                MessageBox.Show(result.ErrorMessage, "Math Matching Impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                UpdateStatusBoard();
            }
        }

        private void RevealMatches_Checked(object sender, RoutedEventArgs e)
        {
            LogsPanel.Visibility = Visibility.Collapsed;
            SecretMatchesPanel.Visibility = Visibility.Visible;
            RightPanelTitle.Text = "🔒 Secret Matches List (Organizer Eyes Only!)";
        }

        private void RevealMatches_Unchecked(object sender, RoutedEventArgs e)
        {
            SecretMatchesPanel.Visibility = Visibility.Collapsed;
            LogsPanel.Visibility = Visibility.Visible;
            RightPanelTitle.Text = "Operation Logs";
        }

        private async void SendEmails_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMatchingResult == null || !_currentMatchingResult.Success)
            {
                MessageBox.Show("Please generate a valid match first.", "Matching Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string email = InputSmtpEmail.Text.Trim();
            string password = _isPasswordRevealed ? InputSmtpPasswordVisible.Text : InputSmtpPassword.Password;
            string organizer = InputOrganizerName.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("SMTP credentials missing. Please configure Gmail Settings first.", "Missing Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Are you sure you want to distribute secret matches to all {_participants.Count} givers now?\n\nThis will send exactly {_participants.Count} individual emails via GMail.", 
                "Required Invitation Confirmation", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes) return;

            // Lock UI navigation during async email run to prevent race states
            SetUiStateRunning(true);
            SendProgressBar.Visibility = Visibility.Visible;
            SendProgressBar.Value = 0;
            
            Log("=============================================");
            Log($"Starting Secret Santa Email Distribution Process for {_participants.Count} participants...");
            Log($"Sender SMTP Account: {email}");
            Log("=============================================");

            int total = _currentMatchingResult.Matches.Count;
            int currentCount = 0;
            int successCount = 0;
            int failedCount = 0;

            string subjectTmpl = TemplateSubject.Text;
            string bodyTmpl = TemplateBody.Text;

            foreach (var match in _currentMatchingResult.Matches)
            {
                string giverId = match.Key;
                string receiverId = match.Value;

                var giver = _participants.FirstOrDefault(p => p.Id == giverId);
                var receiver = _participants.FirstOrDefault(p => p.Id == receiverId);

                if (giver != null && receiver != null)
                {
                    Log($"[{currentCount + 1}/{total}] Preparing match email for {giver.Name} ({giver.Email})...");
                    
                    try
                    {
                        // Send ONLY to the Giver containing details of their specific Receiver
                        await EmailSender.SendEmailAsync(
                            email, 
                            password, 
                            organizer, 
                            giver.Email, 
                            giver.Name, 
                            subjectTmpl, 
                            bodyTmpl, 
                            receiver.Name, 
                            receiver.WishlistUrl
                        );

                        successCount++;
                        Log($"   ↳ [SUCCESS] Secret match dispatched successfully to {giver.Name}!");
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        Log($"   ↳ [FAILED] Could not send to {giver.Name}. Error: {ex.Message}");
                    }
                }

                currentCount++;
                SendProgressBar.Value = (double)currentCount / total * 100;
            }

            Log("=============================================");
            Log("Exchange Email Distribution Completed!");
            Log($"Summary: {successCount} Sent successfully, {failedCount} Failed.");
            Log("=============================================");

            SendProgressBar.Visibility = Visibility.Collapsed;
            SetUiStateRunning(false);

            if (failedCount == 0)
            {
                MessageBox.Show($"All {successCount} Secret Santa emails have been sent successfully! 🎅✨", "Distribution Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Emails sent: {successCount} Succeeded, {failedCount} Failed.\nCheck the Operation Logs panel to review error details for failed members.", "Distribution with Errors", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SetUiStateRunning(bool isRunning)
        {
            TabParticipantsBtn.IsEnabled = !isRunning;
            TabEmailBtn.IsEnabled = !isRunning;
            TabSmtpBtn.IsEnabled = !isRunning;
            TabMatchBtn.IsEnabled = !isRunning;
            SendEmailsBtn.IsEnabled = !isRunning;
        }

        // ==================== CONFIG IMPORT / EXPORT ====================

        private void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    Title = "Export Secret Santa Session",
                    FileName = "secretsanta_exchange.json"
                };

                if (sfd.ShowDialog() == true)
                {
                    var data = GetCurrentSessionData();
                    SessionManager.ExportSession(sfd.FileName, data);
                    Log($"Session exported successfully to: {Path.GetFileName(sfd.FileName)}");
                    MessageBox.Show("Exchange configuration exported successfully. Gmail App Passwords were excluded for safety.", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export session:\n{ex.Message}", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    Title = "Import Secret Santa Session"
                };

                if (ofd.ShowDialog() == true)
                {
                    var session = SessionManager.ImportSession(ofd.FileName);
                    
                    _editingParticipantId = null;
                    SubmitParticipantBtn.Content = "Add Exchange Member";
                    CancelEditBtn.Visibility = Visibility.Collapsed;

                    _participants.Clear();
                    foreach (var p in session.Participants)
                    {
                        _participants.Add(p);
                    }

                    InputOrganizerName.Text = session.OrganizerName;
                    InputSmtpEmail.Text = session.SenderEmail;
                    InputSmtpPassword.Password = string.Empty; // Safety reset
                    CheckboxRememberPass.IsChecked = false;

                    TemplateSubject.Text = session.EmailSubject;
                    TemplateBody.Text = session.EmailBody;

                    _currentMatchingResult = null; // Reset matching state on new import

                    RefreshParticipantsList();
                    UpdateEmailPreview();
                    Log($"Session imported successfully from: {Path.GetFileName(ofd.FileName)}");
                    MessageBox.Show("Exchange configuration loaded! Please re-enter your Gmail App Password for SMTP operations.", "Import Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to import session:\n{ex.Message}", "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== GENERAL LOGGING LOGGER ====================

        private void Log(string message)
        {
            if (LogTextBox == null) return;

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string text = $"[{timestamp}] {message}\n";

            if (LogTextBox.Text == "[Idle] Add participants, adjust templates, and test SMTP credentials to distribute secret matching emails.")
            {
                LogTextBox.Text = text;
            }
            else
            {
                LogTextBox.Text += text;
            }

            LogScrollViewer?.ScrollToEnd();
        }
    }
}