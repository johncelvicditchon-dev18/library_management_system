using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// LIBRARY RESOURCE CENTER EVALUATION FORM.
    /// A public five-section survey (facilities, resources, services,
    /// accessibility and overall satisfaction) that any student, faculty member
    /// or staff member may answer from the welcome page. The whole questionnaire
    /// lives inside an AutoScroll panel, so the SUBMIT button is reached by
    /// scrolling to the bottom.
    ///
    /// The librarian opens one semester at a time in the ANALYTICS tab. The form
    /// only accepts answers while that semester is open, and each account may
    /// submit once per semester, so a new period is needed for a new evaluation.
    /// </summary>
    public partial class EvaluationForm : Form
    {
        private const int RowHeight = 66;
        private const int RowGap = 8;
        private const int SectionHeaderHeight = 46;
        private const int SectionGap = 18;

        // One rating pill and the gap between two pills.
        private const int PillSize = 42;
        private const int PillGap = 8;

        private static readonly string[] RoleOptions =
        {
            "SELECT ROLE",
            "STUDENT",
            "FACULTY",
            "STAFF"
        };

        // The word behind every number, used by the tooltip of each rating pill so
        // the respondent does not have to scroll back up to the legend.
        private static readonly string[] RatingWords =
        {
            string.Empty,
            "POOR",
            "FAIR",
            "GOOD",
            "VERY GOOD",
            "EXCELLENT"
        };

        private static readonly string[] SectionTitles =
        {
            "LIBRARY FACILITIES",
            "LIBRARY RESOURCES",
            "LIBRARY SERVICES",
            "ACCESSIBILITY AND AVAILABILITY",
            "OVERALL USER SATISFACTION"
        };

        private static readonly string[][] Questions =
        {
            new[]
            {
                "The Library Resource Center maintains a clean, orderly, and well-maintained environment.",
                "The library provides adequate seating capacity to accommodate its users.",
                "The library ensures sufficient lighting and proper ventilation within its premises.",
                "The library provides a comfortable and conducive environment for academic study and research.",
                "The library facilities are properly arranged, organized, and maintained to ensure user convenience."
            },
            new[]
            {
                "The library provides an adequate collection of books and reference materials to support academic activities.",
                "The available books and reference materials are relevant to the academic needs of students.",
                "Library resources are systematically organized and readily accessible to users.",
                "The library provides updated and appropriate learning materials that support the academic curriculum.",
                "The available library resources effectively support students' learning, research, and academic development."
            },
            new[]
            {
                "Library personnel demonstrate professionalism, courtesy, and approachability when assisting users.",
                "Library personnel provide timely and appropriate assistance in addressing users' inquiries and concerns.",
                "The procedures for borrowing and returning library materials are convenient and efficiently implemented.",
                "Library services are delivered efficiently and in accordance with established procedures.",
                "Library rules, regulations, and policies are clearly communicated and properly implemented."
            },
            new[]
            {
                "The Library Resource Center is readily accessible to students and other authorized users.",
                "Library resources are sufficiently available to meet the academic and informational needs of users.",
                "The library maintains operating hours that adequately accommodate the schedules of its users.",
                "Users can conveniently locate and access the books and reference materials they require.",
                "Library services are consistently accessible during designated operating hours."
            },
            new[]
            {
                "I am satisfied with the overall condition, organization, and maintenance of the Library Resource Center.",
                "The Library Resource Center effectively meets my academic and informational needs.",
                "The library provides an environment that promotes effective learning, studying, and research.",
                "I am satisfied with the overall quality of the library's facilities, resources, and services.",
                "I would recommend the Library Resource Center to other students as a reliable academic learning resource."
            }
        };

        private static readonly Color CardBorder = Color.FromArgb(226, 232, 240);
        private static readonly Color Green = Color.FromArgb(6, 78, 59);
        private static readonly Color MutedText = Color.FromArgb(100, 116, 139);
        private static readonly Color BodyText = Color.FromArgb(30, 41, 59);

        private readonly EvaluationDirectory _directory = new EvaluationDirectory();
        private readonly List<QuestionRow> _rows = new List<QuestionRow>();
        private readonly List<ContentBlock> _blocks = new List<ContentBlock>();

        // One shared tooltip explains what each number means ("5 - EXCELLENT").
        private readonly ToolTip _ratingTip = new ToolTip
        {
            AutoPopDelay = 8000,
            InitialDelay = 350,
            ReshowDelay = 100,
            ShowAlways = true
        };

        private Panel _content = null!;
        private RoundedCard _respondentCard = null!;
        private Label _roleCaption = null!;
        private Label _nameCaption = null!;
        private Label _idCaption = null!;
        private ComboBox _roleBox = null!;
        private TextBox _nameBox = null!;
        private Label _roleValue = null!;
        private Label _idValue = null!;
        private Label _nameValue = null!;
        private Label _legendLabel = null!;
        private RoundedCard _submitCard = null!;
        private Label _submitNote = null!;
        private RoundedButton _submitButton = null!;

        // Set after a successful LIBRARY EVALUATION sign in. The respondent card
        // then shows ROLE, NAME and — for a student only — STUDENT ID as read only
        // values instead of inputs.
        private bool _signedIn;
        private string _signedInRole = string.Empty;
        private string _signedInIdNumber = string.Empty;
        private string _signedInName = string.Empty;
        private int _accountId;

        // The semester the librarian opened. It is stored with the answers and it
        // is what stops one respondent from answering the same semester twice.
        private readonly int _periodId;
        private readonly string _periodTitle;

        public EvaluationForm()
            : this(null, null, null, 0)
        {
        }

        /// <summary>
        /// Opens the survey for a patron who already signed in through the
        /// LIBRARY EVALUATION pop up. The proven role, ID number and name are shown
        /// read only, the account id is stored with the answers, and the period
        /// identifies the semester the librarian opened for evaluation.
        /// </summary>
        public EvaluationForm(
            string? role, string? idNumber, string? name, int accountId,
            int periodId = 0, string? periodTitle = null)
        {
            InitializeComponent();
            DoubleBuffered = true;

            // The shared rating tooltip is not a child control, so it is released here.
            Disposed += (s, e) => _ratingTip.Dispose();

            // FULL SCREEN: same behaviour as the welcome page. The form opens
            // maximised and F11 toggles the borderless view that covers the monitor.
            ScreenMode.Apply(this);

            _periodId = periodId > 0 ? periodId : 0;
            _periodTitle = (periodTitle ?? string.Empty).Trim();

            _signedInRole = NormalizeRole(role);

            // Only a student carries a student id into the survey; FACULTY and
            // STAFF answers are linked to their account instead.
            _signedInIdNumber = IsStudentRespondent ? (idNumber ?? string.Empty).Trim() : string.Empty;

            _signedInName = (name ?? string.Empty).Trim();
            _accountId = accountId > 0 ? accountId : 0;
            _signedIn = _signedInRole.Length > 0;

            BuildForm();
        }

        /// <summary>
        /// Only a STUDENT has a student id to show. FACULTY and STAFF answers are
        /// recorded against their account, so the ID NO. field is hidden for them.
        /// </summary>
        private bool IsStudentRespondent
        {
            get { return string.Equals(_signedInRole, "STUDENT", StringComparison.OrdinalIgnoreCase); }
        }

        private static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return string.Empty;
            }

            string normalized = role.Trim().ToUpperInvariant();

            // An OFFICER account belongs to the staff group.
            return normalized == "OFFICER" ? "STAFF" : normalized;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Arrange();

            if (_signedIn)
            {
                // WinForms checks the first radio button that receives focus when the
                // window opens. Clear that automatic choice so statement 1 cannot be
                // submitted with a rating the respondent never picked.
                BeginInvoke(new Action(ClearInitialRatings));
            }
            else if (_roleBox != null)
            {
                _roleBox.Focus();
            }
        }

        private void ClearInitialRatings()
        {
            foreach (QuestionRow row in _rows)
            {
                foreach (RadioButton option in row.Options)
                {
                    option.Checked = false;
                }
            }
        }

        // ------------------------------------------------------------------ BUILD

        private void BuildForm()
        {
            _content = new Panel
            {
                BackColor = Color.Transparent,
                Name = "evaluationContent"
            };

            scrollPanel.Controls.Add(_content);

            BuildRespondentCard();
            BuildLegend();
            BuildSections();
            BuildSubmitCard();

            // The cards are measured and positioned by Arrange(), but they still
            // have to be parented to the scrolling content panel to be painted.
            foreach (ContentBlock block in _blocks)
            {
                _content.Controls.Add(block.Control);
            }

            scrollPanel.Resize += (s, e) => Arrange();
            Arrange();
        }

        private void BuildRespondentCard()
        {
            _respondentCard = new RoundedCard
            {
                BackColor = Color.White,
                BorderColor = CardBorder,
                CornerRadius = 14,
                Height = 118
            };

            if (_signedIn)
            {
                BuildSignedInRespondentCard();
            }
            else
            {
                _roleCaption = MakeCaption("I AM A");
                _roleCaption.Location = new Point(20, 16);
                _roleCaption.Size = new Size(260, 18);

                _roleBox = new ComboBox
                {
                    BackColor = Color.FromArgb(243, 244, 246),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10.5F),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Location = new Point(20, 40),
                    Size = new Size(260, 32)
                };
                _roleBox.Items.AddRange(RoleOptions);
                _roleBox.SelectedIndex = 0;

                _nameCaption = MakeCaption("NAME (OPTIONAL)");
                _nameCaption.Location = new Point(306, 16);
                _nameCaption.Size = new Size(300, 18);

                _nameBox = new TextBox
                {
                    BackColor = Color.FromArgb(243, 244, 246),
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 10.5F),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Location = new Point(306, 42),
                    Size = new Size(400, 28)
                };

                _respondentCard.Controls.Add(_roleCaption);
                _respondentCard.Controls.Add(_roleBox);
                _respondentCard.Controls.Add(_nameCaption);
                _respondentCard.Controls.Add(_nameBox);
            }

            _blocks.Add(new ContentBlock(_respondentCard, SectionGap));
        }

        /// <summary>
        /// Signed-in respondent card: ROLE, NAME and, for a student only, the
        /// STUDENT ID are shown as read-only values, because the LIBRARY
        /// EVALUATION sign in already proved them. FACULTY and STAFF have no
        /// student id, so that column is left out of their card.
        /// </summary>
        private void BuildSignedInRespondentCard()
        {
            _roleCaption = MakeCaption("ROLE");
            _roleCaption.Location = new Point(20, 16);
            _roleCaption.Size = new Size(160, 18);

            _roleValue = MakeValueLabel(_signedInRole);
            _roleValue.Location = new Point(20, 40);
            _roleValue.Size = new Size(160, 34);

            // The STUDENT ID column is built for a student only. FACULTY and STAFF
            // have no student id, so the field stays out of their card entirely.
            if (IsStudentRespondent)
            {
                _idCaption = MakeCaption("STUDENT ID");
                _idCaption.Location = new Point(200, 16);
                _idCaption.Size = new Size(230, 18);

                _idValue = MakeValueLabel(string.IsNullOrWhiteSpace(_signedInIdNumber) ? "—" : _signedInIdNumber);
                _idValue.Location = new Point(200, 40);
                _idValue.Size = new Size(230, 34);
            }

            _nameCaption = MakeCaption("NAME");
            _nameCaption.Location = new Point(450, 16);
            _nameCaption.Size = new Size(400, 18);

            _nameValue = MakeValueLabel(string.IsNullOrWhiteSpace(_signedInName) ? "—" : _signedInName);
            _nameValue.Location = new Point(450, 40);
            _nameValue.Size = new Size(400, 34);

            _respondentCard.Controls.Add(_roleCaption);
            _respondentCard.Controls.Add(_roleValue);

            if (_idCaption != null && _idValue != null)
            {
                _respondentCard.Controls.Add(_idCaption);
                _respondentCard.Controls.Add(_idValue);
            }

            _respondentCard.Controls.Add(_nameCaption);
            _respondentCard.Controls.Add(_nameValue);
        }

        private void BuildLegend()
        {
            RoundedCard legendCard = new RoundedCard
            {
                BackColor = Color.FromArgb(240, 253, 244),
                BorderColor = Color.FromArgb(187, 247, 208),
                CornerRadius = 12,
                Height = 54
            };

            _legendLabel = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(4, 120, 87),
                Location = new Point(18, 0),
                Size = new Size(700, 54),
                Text = "RATING SCALE:     5 = EXCELLENT     4 = VERY GOOD     3 = GOOD     2 = FAIR     1 = POOR",
                TextAlign = ContentAlignment.MiddleLeft
            };

            legendCard.Controls.Add(_legendLabel);
            _blocks.Add(new ContentBlock(legendCard, SectionGap));
        }

        private void BuildSections()
        {
            for (int section = 0; section < Questions.Length; section++)
            {
                Label header = new Label
                {
                    AutoSize = false,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                    ForeColor = Green,
                    Padding = new Padding(4, 0, 0, 0),
                    Height = SectionHeaderHeight,
                    Text = (section + 1) + ".   " + SectionTitles[section],
                    TextAlign = ContentAlignment.MiddleLeft
                };

                _blocks.Add(new ContentBlock(header, 4));

                for (int question = 0; question < Questions[section].Length; question++)
                {
                    QuestionRow row = CreateQuestionRow(section, question);
                    _rows.Add(row);

                    bool lastInSection = question == Questions[section].Length - 1;
                    _blocks.Add(new ContentBlock(row.Card, lastInSection ? SectionGap : RowGap));
                }
            }
        }

        private QuestionRow CreateQuestionRow(int section, int question)
        {
            RoundedCard card = new RoundedCard
            {
                BackColor = Color.White,
                BorderColor = CardBorder,
                CornerRadius = 12,
                Height = RowHeight
            };

            Label number = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(4, 120, 87),
                Height = RowHeight,
                Text = (question + 1) + ".",
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 30
            };

            Label text = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = BodyText,
                Height = RowHeight,
                Text = Questions[section][question],
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(number);
            card.Controls.Add(text);

            RadioButton[] options = new RadioButton[5];

            for (int index = 0; index < options.Length; index++)
            {
                int value = 5 - index;

                // RatingPill is a RadioButton, so "one answer per row" and the arrow
                // key navigation keep working exactly as before; only the look changes.
                RatingPill pill = new RatingPill
                {
                    AccessibleName = "Rating " + value + " of 5, " + RatingWords[value],
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    Size = new Size(PillSize, PillSize),
                    Text = value.ToString()
                };

                _ratingTip.SetToolTip(pill, value + " - " + RatingWords[value]);

                options[index] = pill;
                card.Controls.Add(pill);
            }

            QuestionRow row = new QuestionRow(section, question, card, number, text, options);

            // Picking a number clears the red edge a failed submit may have put on
            // this row, so the correction is acknowledged instantly.
            foreach (RadioButton option in options)
            {
                option.CheckedChanged += (s, e) => row.ClearFlag();
            }

            return row;
        }

        private void BuildSubmitCard()
        {
            _submitCard = new RoundedCard
            {
                BackColor = Color.White,
                BorderColor = CardBorder,
                CornerRadius = 14,
                Height = 96
            };

            _submitNote = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = MutedText,
                Location = new Point(20, 25),
                Size = new Size(500, 46),
                Text = _periodTitle.Length > 0
                    ? "ALL 25 STATEMENTS MUST BE RATED BEFORE SUBMITTING.\nEVALUATION FOR " + _periodTitle + "."
                    : "ALL 25 STATEMENTS MUST BE RATED BEFORE SUBMITTING.",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _submitButton = new RoundedButton
            {
                BackColor = Color.Transparent,
                CornerRadius = 12,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(21, 128, 61),
                NormalColor = Color.FromArgb(22, 163, 74),
                PressedColor = Color.FromArgb(20, 83, 45),
                Size = new Size(220, 46),
                Text = "SUBMIT EVALUATION",
                UseVisualStyleBackColor = false
            };
            _submitButton.FlatAppearance.BorderSize = 0;
            _submitButton.Click += SubmitButton_Click;

            _submitCard.Controls.Add(_submitNote);
            _submitCard.Controls.Add(_submitButton);

            _blocks.Add(new ContentBlock(_submitCard, 0));
        }

        private static Label MakeCaption(string text)
        {
            return new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                ForeColor = MutedText,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Label MakeValueLabel(string text)
        {
            return new Label
            {
                AutoEllipsis = true,
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = BodyText,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        // ----------------------------------------------------------------- LAYOUT

        private void Arrange()
        {
            if (_content == null || scrollPanel == null)
            {
                return;
            }

            int width = scrollPanel.ClientSize.Width - scrollPanel.Padding.Horizontal;

            if (scrollPanel.VerticalScroll.Visible)
            {
                width -= SystemInformation.VerticalScrollBarWidth;
            }

            width = Math.Max(620, width);

            int top = 0;

            foreach (ContentBlock block in _blocks)
            {
                block.Control.Left = 0;
                block.Control.Top = top;
                block.Control.Width = width;
                top += block.Control.Height + block.Gap;
            }

            foreach (QuestionRow row in _rows)
            {
                row.Layout(width);
            }

            ArrangeRespondentCard(width);
            ArrangeLegend(width);
            ArrangeSubmitCard(width);

            _content.Location = new Point(scrollPanel.Padding.Left, scrollPanel.Padding.Top);
            _content.Size = new Size(width, top);
        }

        private void ArrangeRespondentCard(int width)
        {
            if (_signedIn)
            {
                const int roleWidth = 160;
                const int idWidth = 230;
                const int gap = 20;

                _roleCaption.Width = roleWidth;
                _roleValue.Width = roleWidth;

                int nameLeft;

                if (_idCaption != null && _idValue != null)
                {
                    int idLeft = 20 + roleWidth + gap;

                    _idCaption.Left = idLeft;
                    _idCaption.Width = idWidth;
                    _idValue.Left = idLeft;
                    _idValue.Width = idWidth;

                    nameLeft = idLeft + idWidth + gap;
                }
                else
                {
                    // No STUDENT ID column: the NAME value takes the free space.
                    nameLeft = 20 + roleWidth + gap;
                }

                int nameWidth = Math.Max(160, width - nameLeft - 20);

                _nameCaption.Left = nameLeft;
                _nameCaption.Width = nameWidth;
                _nameValue.Left = nameLeft;
                _nameValue.Width = nameWidth;
                return;
            }

            const int roleInputWidth = 260;

            _roleCaption.Width = roleInputWidth;
            _roleBox.Width = roleInputWidth;

            int inputNameLeft = 20 + roleInputWidth + 24;
            int inputNameWidth = Math.Max(160, width - inputNameLeft - 20);

            _nameCaption.Left = inputNameLeft;
            _nameCaption.Width = inputNameWidth;
            _nameBox.Left = inputNameLeft;
            _nameBox.Width = inputNameWidth;
        }

        private void ArrangeLegend(int width)
        {
            _legendLabel.Width = Math.Max(400, width - 36);
        }

        private void ArrangeSubmitCard(int width)
        {
            _submitButton.Left = Math.Max(20, width - _submitButton.Width - 22);
            _submitButton.Top = (_submitCard.Height - _submitButton.Height) / 2;

            _submitNote.Left = 20;
            _submitNote.Top = 25;
            _submitNote.Width = Math.Max(180, _submitButton.Left - 40);
        }

        // ----------------------------------------------------------------- SUBMIT

        private void SubmitButton_Click(object? sender, EventArgs e)
        {
            if (!_signedIn && _roleBox.SelectedIndex <= 0)
            {
                Authentication.ShowError("EVALUATION", "PLEASE SELECT WHETHER YOU ARE A STUDENT, FACULTY OR STAFF.");
                _roleBox.Focus();
                return;
            }

            int[] ratings = new int[25];
            QuestionRow? firstMissing = null;

            foreach (QuestionRow row in _rows)
            {
                int rating = 0;

                for (int index = 0; index < row.Options.Length; index++)
                {
                    if (row.Options[index].Checked)
                    {
                        rating = 5 - index;
                        break;
                    }
                }

                if (rating == 0 && firstMissing == null)
                {
                    firstMissing = row;
                }

                // Unanswered rows get a red edge so the respondent can see, at a
                // glance, which statements still need a number.
                if (rating == 0)
                {
                    row.FlagMissing();
                }
                else
                {
                    row.ClearFlag();
                }

                ratings[row.Index] = rating;
            }

            if (firstMissing != null)
            {
                Authentication.ShowError("EVALUATION", "PLEASE RATE EVERY STATEMENT BEFORE SUBMITTING. THE UNANSWERED ROWS ARE MARKED IN RED.");
                scrollPanel.ScrollControlIntoView(firstMissing.Card);
                return;
            }

            string role = _signedIn
                ? _signedInRole
                : (Convert.ToString(_roleBox.SelectedItem) ?? "STUDENT");
            string respondentName = _signedIn ? _signedInName : _nameBox.Text.Trim();

            try
            {
                _directory.Submit(role, respondentName, ratings, _accountId, _periodId);

                Authentication.ShowMessage(
                    "THANK YOU! YOUR RESPONSE HAS BEEN RECORDED.",
                    "EVALUATION SUBMITTED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                // The semester was closed, or this account already answered it.
                Authentication.ShowError("EVALUATION", ex.Message.ToUpperInvariant());
            }
            catch (ArgumentException ex)
            {
                Authentication.ShowError("EVALUATION", ex.Message);
            }
            catch (MySqlException ex)
            {
                Authentication.ShowError("EVALUATION", "YOUR EVALUATION COULD NOT BE SAVED.\n" + ex.Message);
            }
        }

        private sealed class ContentBlock
        {
            public ContentBlock(Control control, int gap)
            {
                Control = control;
                Gap = gap;
            }

            public Control Control { get; }
            public int Gap { get; }
        }

        private sealed class QuestionRow
        {
            private static readonly Color FlagBorder = Color.FromArgb(248, 113, 113);
            private static readonly Color FlagNumber = Color.FromArgb(220, 38, 38);

            private readonly Color _normalBorder;
            private readonly Color _normalNumber;
            private bool _flagged;

            public QuestionRow(int section, int question, RoundedCard card, Label number, Label text, RadioButton[] options)
            {
                Section = section;
                Question = question;
                Card = card;
                Number = number;
                Text = text;
                Options = options;

                _normalBorder = card.BorderColor;
                _normalNumber = number.ForeColor;
            }

            public int Section { get; }
            public int Question { get; }
            public int Index
            {
                get { return (Section * 5) + Question; }
            }

            public RoundedCard Card { get; }
            public Label Number { get; }
            public Label Text { get; }
            public RadioButton[] Options { get; }

            /// <summary>Red edge used when a submit attempt found this row blank.</summary>
            public void FlagMissing()
            {
                if (_flagged)
                {
                    return;
                }

                _flagged = true;
                Card.BorderColor = FlagBorder;
                Number.ForeColor = FlagNumber;
            }

            /// <summary>Puts the row back to its normal look once it has an answer.</summary>
            public void ClearFlag()
            {
                if (!_flagged)
                {
                    return;
                }

                _flagged = false;
                Card.BorderColor = _normalBorder;
                Number.ForeColor = _normalNumber;
            }

            public void Layout(int width)
            {
                Card.Width = width;

                Number.Left = 18;
                Number.Top = 0;
                Number.Width = 30;
                Number.Height = Card.Height;

                const int optionWidth = PillSize;
                int optionsWidth = (optionWidth * Options.Length) + (PillGap * (Options.Length - 1));
                int optionsLeft = width - optionsWidth - 18;

                Text.Left = 48;
                Text.Top = 0;
                Text.Width = Math.Max(150, optionsLeft - Text.Left - 16);
                Text.Height = Card.Height;

                for (int index = 0; index < Options.Length; index++)
                {
                    Options[index].Left = optionsLeft + (index * (optionWidth + PillGap));
                    Options[index].Top = (Card.Height - Options[index].Height) / 2;
                }
            }
        }
    }
}
