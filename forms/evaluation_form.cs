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
    /// </summary>
    public partial class EvaluationForm : Form
    {
        private const int RowHeight = 66;
        private const int RowGap = 8;
        private const int SectionHeaderHeight = 46;
        private const int SectionGap = 18;

        private static readonly string[] RoleOptions =
        {
            "SELECT ROLE",
            "STUDENT",
            "FACULTY",
            "STAFF"
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

        private Panel _content = null!;
        private RoundedCard _respondentCard = null!;
        private Label _roleCaption = null!;
        private Label _nameCaption = null!;
        private ComboBox _roleBox = null!;
        private TextBox _nameBox = null!;
        private Label _legendLabel = null!;
        private RoundedCard _submitCard = null!;
        private Label _submitNote = null!;
        private RoundedButton _submitButton = null!;

        public EvaluationForm()
        {
            InitializeComponent();
            BuildForm();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Arrange();
            _roleBox.Focus();
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

            _blocks.Add(new ContentBlock(_respondentCard, SectionGap));
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

                options[index] = new RadioButton
                {
                    AccessibleName = "Rating " + value,
                    AutoSize = false,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(51, 65, 85),
                    Size = new Size(34, 22),
                    TabStop = true,
                    Text = value.ToString(),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                card.Controls.Add(options[index]);
            }

            return new QuestionRow(section, question, card, number, text, options);
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
                Text = "ALL 25 STATEMENTS MUST BE RATED BEFORE SUBMITTING.",
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
            const int roleWidth = 260;

            _roleCaption.Width = roleWidth;
            _roleBox.Width = roleWidth;

            int nameLeft = 20 + roleWidth + 24;
            int nameWidth = Math.Max(160, width - nameLeft - 20);

            _nameCaption.Left = nameLeft;
            _nameCaption.Width = nameWidth;
            _nameBox.Left = nameLeft;
            _nameBox.Width = nameWidth;
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
            if (_roleBox.SelectedIndex <= 0)
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

                ratings[row.Index] = rating;
            }

            if (firstMissing != null)
            {
                Authentication.ShowError("EVALUATION", "PLEASE RATE EVERY STATEMENT BEFORE SUBMITTING.");
                scrollPanel.ScrollControlIntoView(firstMissing.Card);
                return;
            }

            string role = Convert.ToString(_roleBox.SelectedItem) ?? "STUDENT";

            try
            {
                _directory.Submit(role, _nameBox.Text.Trim(), ratings);

                Authentication.ShowMessage(
                    "THANK YOU! YOUR RESPONSE HAS BEEN RECORDED.",
                    "EVALUATION SUBMITTED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
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
            public QuestionRow(int section, int question, RoundedCard card, Label number, Label text, RadioButton[] options)
            {
                Section = section;
                Question = question;
                Card = card;
                Number = number;
                Text = text;
                Options = options;
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

            public void Layout(int width)
            {
                Card.Width = width;

                Number.Left = 18;
                Number.Top = 0;
                Number.Width = 30;
                Number.Height = Card.Height;

                const int optionWidth = 34;
                int optionsLeft = width - (optionWidth * Options.Length) - 16;

                Text.Left = 48;
                Text.Top = 0;
                Text.Width = Math.Max(150, optionsLeft - Text.Left - 16);
                Text.Height = Card.Height;

                for (int index = 0; index < Options.Length; index++)
                {
                    Options[index].Left = optionsLeft + (index * optionWidth);
                    Options[index].Top = (Card.Height - Options[index].Height) / 2;
                }
            }
        }
    }
}
