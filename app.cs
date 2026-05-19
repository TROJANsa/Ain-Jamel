using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Diagnostics; 
using Microsoft.Win32;

namespace ElderlyReminder
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SplashScreen splash = new SplashScreen();
            splash.Show();
            
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < 5)
            {
                double progress = (DateTime.Now - startTime).TotalSeconds / 5.0;
                splash.UpdateProgressBar(progress);
                Application.DoEvents();
                System.Threading.Thread.Sleep(30);
            }

            while (splash.Opacity > 0)
            {
                splash.Opacity -= 0.05;
                System.Threading.Thread.Sleep(20);
                Application.DoEvents();
            }
            splash.Close();

            Application.Run(new ReminderForm());
        }
    }

    public class SplashScreen : Form
    {
        private Panel pnlProgress;
        private int maxBarWidth = 300;

        public SplashScreen()
        {
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; 
            this.BackColor = Color.FromArgb(18, 18, 18); 
            this.ShowInTaskbar = false;

            Label lblWelcome = new Label
            {
                Location = new Point(0, 80),
                Size = new Size(500, 60),
                Text = "WELCOME",
                Font = new Font("Segoe UI Light", 36, FontStyle.Regular),
                ForeColor = Color.FromArgb(249, 246, 238), 
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblWelcome);

            Panel pnlTrack = new Panel
            {
                Location = new Point(100, 170),
                Size = new Size(maxBarWidth, 4),
                BackColor = Color.FromArgb(40, 40, 40)
            };
            this.Controls.Add(pnlTrack);

            pnlProgress = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(0, 4),
                BackColor = Color.FromArgb(249, 246, 238)
            };
            pnlTrack.Controls.Add(pnlProgress);
        }

        public void UpdateProgressBar(double progress)
        {
            if (progress > 1.0) progress = 1.0;
            pnlProgress.Width = (int)(maxBarWidth * progress);
        }
    }

    public class ReminderForm : Form
    {
        private Panel pnlReminderView;
        private Panel pnlAzkarView;
        private Panel pnlGameView;

        private Button btnNavReminder, btnNavAzkar, btnNavGame;

        private Label lblTitle, lblTextHint, lblTypeHint, lblTimeHint, lblMasbaha, lblTasbeehCount, lblAzkar;
        private TextBox txtReminder;
        private ComboBox comboType;
        private NumericUpDown numMinutes;
        private DateTimePicker datePicker; 
        private Button btnActivate, btnPermanentSave, btnLangToggle, btnNextZikr, btnTasbeeh, btnCloseApp;
        private Panel panelCredits, panelCenterWrapper;
        private Label lblDev, lblGraphics1, lblGraphics2, lblQemaTeam;

        private Label lblGameTitle, lblQuestion, lblScore;
        private TextBox txtAnswer;
        private Button btnCheckAnswer, btnNextQuestion;
        private int correctAnswer = 0;
        private int gameScore = 0;
        private Random random = new Random();

        private Timer systemTimer;
        private int countdownSeconds = 0;
        private DateTime staticTargetTime;
        private string reminderText = "";
        private string selectedMode = "Countdown";
        private bool isEnglish = true; 
        private int tasbeehCount = 0;

        private string[] azkarAr = new string[] {
            "☀️ [أذكار الصباح]: أَصْبَحْنَا وَأَصْبَحَ الْمُلْكُ لِلَّهِ وَالْحَمْدُ لِلَّهِ لا إِلَهَ إِلا اللَّهُ",
            "🌙 [أذكار المساء]: أَمْسَيْنَا وَأَمْسَى الْمُلْكُ لِلَّهِ وَالْحَمْدُ لِلَّهِ لا إِلَهَ إِلا اللَّهُ",
            "سُبْحَانَ اللَّهِ وَبِحَمْدِهِ ، سُبْحَانَ اللَّهِ الْعَظِيمِ",
            "أَسْتَغْفِرُ اللَّهَ الْعَظِيمَ وَأَتُوبُ إِلَيْهِ",
            "اللَّهُمَّ صَلِّ وَسَلِّمْ عَلَى نَبِيِّنَا مُحَمَّدٍ",
            "لَا حَوْلَ وَلَا قُوَّةَ إِلَّا بِاللَّهِ الْعَلِيِّ الْعَظِيمِ"
        };
        private string[] azkarEn = new string[] {
            "☀️ [Morning]: We have entered a new day and with it all dominion belongs to Allah.",
            "🌙 [Evening]: We have entered the evening and with it all dominion belongs to Allah.",
            "Glory be to Allah and His is the praise, Glory be to Allah the Most Great.",
            "I seek the forgiveness of Allah the Almighty, and I turn to Him in repentance.",
            "O Allah, send prayers and peace upon our Prophet Muhammad.",
            "There is no might nor power except with Allah, the Most High, the Most Great."
        };
        private int currentZikrIndex = 0;

        public ReminderForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(18, 18, 18); 
            this.Icon = new System.Drawing.Icon(@"icon.ico");
            this.ShowInTaskbar = true;
            this.Text = "AinJamel";

            panelCenterWrapper = new Panel { Size = new Size(900, 650), BackColor = Color.FromArgb(18, 18, 18) };
            this.Controls.Add(panelCenterWrapper);
            this.Load += (s, e) => {
                panelCenterWrapper.Location = new Point((this.Width - panelCenterWrapper.Width) / 2, (this.Height - panelCenterWrapper.Height) / 2);
            };

            btnCloseApp = new Button { Location = new Point(850, 10), Size = new Size(40, 30), Text = "X", Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.FromArgb(249, 246, 238), FlatStyle = FlatStyle.Flat };
            btnCloseApp.FlatAppearance.BorderSize = 0;
            btnCloseApp.Click += (s, e) => Application.Exit();
            panelCenterWrapper.Controls.Add(btnCloseApp);

            btnNavReminder = new Button { Location = new Point(10, 50), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnNavReminder.FlatAppearance.BorderSize = 0;
            btnNavReminder.Click += (s, e) => SwitchView(1);

            btnNavAzkar = new Button { Location = new Point(155, 50), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnNavAzkar.FlatAppearance.BorderSize = 0;
            btnNavAzkar.Click += (s, e) => SwitchView(2);

            btnNavGame = new Button { Location = new Point(300, 50), Size = new Size(140, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnNavGame.FlatAppearance.BorderSize = 0;
            btnNavGame.Click += (s, e) => SwitchView(3);

            panelCenterWrapper.Controls.AddRange(new Control[] { btnNavReminder, btnNavAzkar, btnNavGame });

            pnlReminderView = new Panel { Size = new Size(880, 340), Location = new Point(10, 95), BackColor = Color.FromArgb(24, 24, 24) };
            pnlAzkarView = new Panel { Size = new Size(880, 340), Location = new Point(10, 95), BackColor = Color.FromArgb(24, 24, 24), Visible = false };
            pnlGameView = new Panel { Size = new Size(880, 340), Location = new Point(10, 95), BackColor = Color.FromArgb(24, 24, 24), Visible = false };
            
            panelCenterWrapper.Controls.AddRange(new Control[] { pnlReminderView, pnlAzkarView, pnlGameView });

            lblTitle = new Label { Location = new Point(20, 20), Width = 840, Height = 40, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(249, 246, 238), TextAlign = ContentAlignment.MiddleCenter };
            lblTextHint = new Label { Location = new Point(60, 80), Width = 350, Height = 25, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
            txtReminder = new TextBox { Location = new Point(60, 110), Width = 350, Font = new Font("Segoe UI", 13), BackColor = Color.FromArgb(35, 35, 35), ForeColor = Color.FromArgb(249, 246, 238), BorderStyle = BorderStyle.FixedSingle };
            
            lblTypeHint = new Label { Location = new Point(440, 80), Width = 160, Height = 25, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
            comboType = new ComboBox { Location = new Point(440, 110), Width = 160, Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 35, 35), ForeColor = Color.FromArgb(249, 246, 238) };
            comboType.SelectedIndexChanged += ComboType_SelectedIndexChanged;

            lblTimeHint = new Label { Location = new Point(630, 80), Width = 180, Height = 25, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
            numMinutes = new NumericUpDown { Location = new Point(630, 110), Width = 180, Font = new Font("Segoe UI", 12), Minimum = 1, Maximum = 600, Value = 1, BackColor = Color.FromArgb(35, 35, 35), ForeColor = Color.FromArgb(249, 246, 238) };
            datePicker = new DateTimePicker { Location = new Point(630, 110), Width = 180, Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy/MM/dd  hh:mm tt", Visible = false, BackColor = Color.FromArgb(35, 35, 35) };

            btnActivate = new Button { Location = new Point(60, 190), Width = 750, Height = 45, Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = Color.FromArgb(249, 246, 238), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
            btnActivate.FlatAppearance.BorderSize = 0;
            btnActivate.Click += BtnActivate_Click;

            btnPermanentSave = new Button { Location = new Point(60, 255), Width = 750, Height = 45, Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(35, 35, 35), ForeColor = Color.FromArgb(249, 246, 238), FlatStyle = FlatStyle.Flat };
            btnPermanentSave.FlatAppearance.BorderColor = Color.FromArgb(249, 246, 238);
            btnPermanentSave.Click += BtnPermanentSave_Click;

            pnlReminderView.Controls.AddRange(new Control[] { lblTitle, lblTextHint, txtReminder, lblTypeHint, comboType, lblTimeHint, numMinutes, datePicker, btnActivate, btnPermanentSave });

            lblAzkar = new Label { Location = new Point(40, 30), Width = 790, Height = 80, Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(249, 246, 238), TextAlign = ContentAlignment.MiddleCenter };
            btnNextZikr = new Button { Location = new Point(335, 130), Width = 200, Height = 40, Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(35, 35, 35), ForeColor = Color.FromArgb(249, 246, 238), FlatStyle = FlatStyle.Flat };
            btnNextZikr.FlatAppearance.BorderSize = 0;
            btnNextZikr.Click += (s, e) => {
                currentZikrIndex = (currentZikrIndex + 1) % azkarAr.Length;
                lblAzkar.Text = isEnglish ? azkarEn[currentZikrIndex] : azkarAr[currentZikrIndex];
            };

            lblMasbaha = new Label { Location = new Point(40, 195), Width = 790, Height = 30, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(249, 246, 238), TextAlign = ContentAlignment.MiddleCenter };
            btnTasbeeh = new Button { Location = new Point(375, 235), Width = 120, Height = 45, Font = new Font("Segoe UI", 13, FontStyle.Bold), BackColor = Color.FromArgb(249, 246, 238), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
            btnTasbeeh.FlatAppearance.BorderSize = 0;
            lblTasbeehCount = new Label { Location = new Point(335, 290), Width = 200, Height = 30, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter };
            btnTasbeeh.Click += (s, e) => { tasbeehCount++; lblTasbeehCount.Text = (isEnglish ? "Count: " : "العدد: ") + tasbeehCount; };

            pnlAzkarView.Controls.AddRange(new Control[] { lblAzkar, btnNextZikr, lblMasbaha, btnTasbeeh, lblTasbeehCount });

            lblGameTitle = new Label { Location = new Point(40, 20), Width = 790, Height = 40, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(249, 246, 238), TextAlign = ContentAlignment.MiddleCenter };
            lblQuestion = new Label { Location = new Point(40, 80), Width = 790, Height = 45, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.FromArgb(249, 246, 238), TextAlign = ContentAlignment.MiddleCenter };
            
            txtAnswer = new TextBox { Location = new Point(340, 145), Width = 200, Font = new Font("Segoe UI", 16), BackColor = Color.FromArgb(35, 35, 35), ForeColor = Color.FromArgb(249, 246, 238), BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            
            btnCheckAnswer = new Button { Location = new Point(340, 205), Width = 200, Height = 40, Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(249, 246, 238), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
            btnCheckAnswer.FlatAppearance.BorderSize = 0;
            btnCheckAnswer.Click += BtnCheckAnswer_Click;

            lblScore = new Label { Location = new Point(40, 265), Width = 790, Height = 30, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter };

            pnlGameView.Controls.AddRange(new Control[] { lblGameTitle, lblQuestion, txtAnswer, btnCheckAnswer, lblScore });
            GenerateMathQuestion(); 

            panelCredits = new Panel { Location = new Point(10, 485), Size = new Size(880, 155), BackColor = Color.FromArgb(12, 12, 12) };
            
            btnLangToggle = new Button { Location = new Point(380, 10), Width = 120, Height = 35, Text = "AR | ENG", Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(35, 35, 35), ForeColor = Color.FromArgb(249, 246, 238), FlatStyle = FlatStyle.Flat };
            btnLangToggle.FlatAppearance.BorderSize = 0;
            btnLangToggle.Click += (s, e) => { isEnglish = !isEnglish; UpdateLanguageUI(); };

            lblDev = new Label { Location = new Point(10, 52), Width = 860, Height = 25, Text = "By devYasser AL Qahtani", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(249, 246, 238), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
            lblDev.Click += (s, e) => { Process.Start(new ProcessStartInfo("https://github.com/TROJANsa") { UseShellExecute = true }); };

            lblGraphics1 = new Label { Location = new Point(10, 84), Width = 860, Height = 18, Text = "Graphics team", Font = new Font("Arial", 8, FontStyle.Regular), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleCenter };
            lblGraphics2 = new Label { Location = new Point(10, 102), Width = 860, Height = 18, Text = "Hamdi Emad El-Din", Font = new Font("Arial", 8, FontStyle.Regular), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleCenter };

            lblQemaTeam = new Label { Location = new Point(10, 124), Width = 860, Height = 18, Text = "QEMA TEAM", Font = new Font("Arial", 8, FontStyle.Bold), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
            lblQemaTeam.Click += (s, e) => { Process.Start(new ProcessStartInfo("https://github.com/TROJANsa") { UseShellExecute = true }); };

            panelCredits.Controls.AddRange(new Control[] { btnLangToggle, lblDev, lblGraphics1, lblGraphics2, lblQemaTeam });
            panelCenterWrapper.Controls.Add(panelCredits);

            SwitchView(1); 
            UpdateLanguageUI();

            systemTimer = new Timer();
            systemTimer.Interval = 1000;
            systemTimer.Tick += SystemTimer_Tick;
        }

        private void GenerateMathQuestion()
        {
            int num1 = random.Next(5, 25);
            int num2 = random.Next(3, 15);
            int operation = random.Next(0, 2);

            if (operation == 0)
            {
                lblQuestion.Text = num1 + " + " + num2 + " = ?";
                correctAnswer = num1 + num2;
            }
            else
            {
                if (num1 < num2) { int temp = num1; num1 = num2; num2 = temp; }
                lblQuestion.Text = num1 + " - " + num2 + " = ?";
                correctAnswer = num1 - num2;
            }
            txtAnswer.Text = "";
            txtAnswer.Focus();
        }

        private void BtnCheckAnswer_Click(object sender, EventArgs e)
        {
            int userAnswer = 0;
            if (int.TryParse(txtAnswer.Text.Trim(), out userAnswer))
            {
                if (userAnswer == correctAnswer)
                {
                    gameScore += 10;
                    MessageBox.Show(isEnglish ? "Excellent! Correct Answer (+10)" : "ممتاز جداً! إجابة صحيحة وتنشيط رائع للمخ (+10)");
                }
                else
                {
                    MessageBox.Show(isEnglish ? "Try again! The correct answer was: " + correctAnswer : "محاولة أخرى! الإجابة الصحيحة كانت: " + correctAnswer);
                }
                lblScore.Text = (isEnglish ? "Score: " : "النقاط الحالية: ") + gameScore;
                GenerateMathQuestion();
            }
        }

        private void SwitchView(int tabIndex)
        {
            pnlReminderView.Visible = (tabIndex == 1);
            pnlAzkarView.Visible = (tabIndex == 2);
            pnlGameView.Visible = (tabIndex == 3);

            btnNavReminder.BackColor = (tabIndex == 1) ? Color.FromArgb(249, 246, 238) : Color.FromArgb(30, 30, 30);
            btnNavReminder.ForeColor = (tabIndex == 1) ? Color.Black : Color.FromArgb(249, 246, 238);

            btnNavAzkar.BackColor = (tabIndex == 2) ? Color.FromArgb(249, 246, 238) : Color.FromArgb(30, 30, 30);
            btnNavAzkar.ForeColor = (tabIndex == 2) ? Color.Black : Color.FromArgb(249, 246, 238);

            btnNavGame.BackColor = (tabIndex == 3) ? Color.FromArgb(249, 246, 238) : Color.FromArgb(30, 30, 30);
            btnNavGame.ForeColor = (tabIndex == 3) ? Color.Black : Color.FromArgb(249, 246, 238);
        }

        private void UpdateLanguageUI()
        {
            if (isEnglish)
            {
                this.RightToLeft = RightToLeft.No;
                this.RightToLeftLayout = false;
                btnNavReminder.Text = "⏰ Reminders";
                btnNavAzkar.Text = "🕋 Azkar";
                btnNavGame.Text = "🧠 Brain Game";
                
                lblTitle.Text = "Smart Scheduling & Reminder Assistant";
                lblTextHint.Text = "Reminder Message:";
                if(txtReminder.Text == "اكتب ما تريد فعله في هذا الوقت" || txtReminder.Text == "") txtReminder.Text = "";
                lblTypeHint.Text = "Alert Mode:";
                
                comboType.Items.Clear();
                comboType.Items.AddRange(new string[] { "Specific Time", "Countdown" });
                comboType.SelectedIndex = selectedMode == "Specific Time" ? 0 : 1;

                lblTimeHint.Text = selectedMode == "Specific Time" ? "Select Date/Time:" : "Minutes:";
                btnActivate.Text = "Save & Run in Background";
                btnPermanentSave.Text = "💾 Permanent Save (Startup Auto-Run)";

                lblAzkar.Text = azkarEn[currentZikrIndex];
                lblAzkar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                btnNextZikr.Text = "Next Verse ↩";
                lblMasbaha.Text = "📿 Digital Tasbeeh";
                btnTasbeeh.Text = "PRAISE";
                lblTasbeehCount.Text = "Count: " + tasbeehCount;

                lblGameTitle.Text = "Brain Activation Game (Math Speed)";
                btnCheckAnswer.Text = "SUBMIT ANSWER";
                lblScore.Text = "Score: " + gameScore;
            }
            else
            {
                this.RightToLeft = RightToLeft.Yes;
                this.RightToLeftLayout = true;
                btnNavReminder.Text = "⏰ ضبط التذكيرات";
                btnNavAzkar.Text = "🕋 الأذكار";
                btnNavGame.Text = "🧠 ألعاب الذاكرة";
                
                lblTitle.Text = "منظّم المواعيد والتذكيرات الذكي";
                lblTextHint.Text = "نص التذكير:";
                if(txtReminder.Text == "What do you want to do?" || txtReminder.Text == "") txtReminder.Text = "";
                lblTypeHint.Text = "نوع التنبيه:";
                
                comboType.Items.Clear();
                comboType.Items.AddRange(new string[] { "وقت محدد", "مؤقت (بعد)" });
                comboType.SelectedIndex = selectedMode == "وقت محدد" ? 0 : 1;

                lblTimeHint.Text = selectedMode == "وقت محدد" ? "اختر التاريخ والوقت:" : "الدقائق:";
                btnActivate.Text = "حفظ وتفعيل البرنامج في الخلفية";
                btnPermanentSave.Text = "💾 الحفظ الدائم (التشغيل التلقائي مع الويندوز)";

                lblAzkar.Text = azkarAr[currentZikrIndex];
                lblAzkar.Font = new Font("Traditional Arabic", 16, FontStyle.Bold);
                btnNextZikr.Text = "الذكر التالي ↩";
                lblMasbaha.Text = "📿 المسبحة الإلكترونية";
                btnTasbeeh.Text = "سَبِّحْ";
                lblTasbeehCount.Text = "العدد: " + tasbeehCount;

                lblGameTitle.Text = "لعبة تنشيط المخ وسرعة البديهة";
                btnCheckAnswer.Text = "تحقق من الإجابة";
                lblScore.Text = "النقاط الحالية: " + gameScore;
            }
        }

        private void ComboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboType.SelectedIndex == 0) 
            {
                selectedMode = isEnglish ? "Specific Time" : "وقت محدد";
                numMinutes.Visible = false;
                datePicker.Visible = true;
                lblTimeHint.Text = isEnglish ? "Select Date/Time:" : "اختر التاريخ والوقت:";
            }
            else 
            {
                selectedMode = isEnglish ? "Countdown" : "مؤقت (بعد)";
                datePicker.Visible = false;
                numMinutes.Visible = true;
                lblTimeHint.Text = isEnglish ? "Minutes:" : "الدقائق:";
            }
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            reminderText = txtReminder.Text.Trim();
            if (string.IsNullOrEmpty(reminderText) || reminderText == "What do you want to do?" || reminderText == "اكتب ما تريد فعله في هذا الوقت")
            {
                MessageBox.Show(isEnglish ? "Please enter a reminder message first." : "الرجاء كتابة نص التذكير أولاً.");
                return;
            }

            if (selectedMode.Contains("Countdown") || selectedMode.Contains("مؤقت"))
            {
                countdownSeconds = (int)numMinutes.Value * 60;
                systemTimer.Start();
            }
            else 
            {
                staticTargetTime = datePicker.Value;
                if (staticTargetTime <= DateTime.Now)
                {
                    MessageBox.Show(isEnglish ? "Please pick a future date and time." : "الرجاء اختيار وقت وتاريخ مستقبلي لحجز الموعد بنجاح.");
                    return;
                }
                systemTimer.Start();
            }

            MessageBox.Show(isEnglish ? "Reminder saved! The app is now hidden running securely." : "تم حفظ التذكير وتفعيل وضع الخلفية الذكي بنجاح!");
            this.Hide();
        }

        private void BtnPermanentSave_Click(object sender, EventArgs e)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                rk.SetValue("ElderlyReminderApp", Application.ExecutablePath);
                MessageBox.Show(isEnglish ? "Successfully Added! App will auto-run on Windows startup." : "تمت الإضافة الدائمة! سيفتح البرنامج تلقائياً فور تشغيل جهاز الكمبيوتر.");
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void SystemTimer_Tick(object sender, EventArgs e)
        {
            if (selectedMode.Contains("Countdown") || selectedMode.Contains("مؤقت"))
            {
                if (countdownSeconds > 0) countdownSeconds--;
                else { systemTimer.Stop(); TriggerAlert(); }
            }
            else 
            {
                DateTime now = DateTime.Now;
                if (now.Year == staticTargetTime.Year && now.Month == staticTargetTime.Month && 
                    now.Day == staticTargetTime.Day && now.Hour == staticTargetTime.Hour && 
                    now.Minute == staticTargetTime.Minute)
                {
                    systemTimer.Stop();
                    TriggerAlert();
                }
            }
        }

        private void TriggerAlert()
        {
            this.Show();
            this.WindowState = FormWindowState.Maximized; 
            this.Activate();
            this.TopMost = true;

            try
            {
                if (File.Exists("sound.wav"))
                {
                    SoundPlayer player = new SoundPlayer("sound.wav");
                    player.Play();
                }
                else { System.Media.SystemSounds.Exclamation.Play(); }
            }
            catch { System.Media.SystemSounds.Exclamation.Play(); }

            MessageBox.Show(this, (isEnglish ? "Time for:\n\n" : "حان الآن موعد:\n\n") + reminderText, isEnglish ? "Important Reminder ⏰" : "تذكير هام جداً ⏰", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, isEnglish ? MessageBoxOptions.DefaultDesktopOnly : MessageBoxOptions.RightAlign);
            
            this.TopMost = false;
        }
    }
}
