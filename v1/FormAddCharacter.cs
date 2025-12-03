using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Windows.Forms;

namespace AutoVPT
{
    public partial class FormAddCharacter : Form
    {
        Character character;
        public string item;
        private string originalCharacterId = null; // Track original ID for edit mode

        public FormAddCharacter()
        {
            InitializeComponent();
        }

        private void buttonAddNewCharacter_Click(object sender, EventArgs e)
        {
            SaveOrUpdateData();
        }

        public void SaveOrUpdateData()
        {
            if (isValidate())
            {
                SaveOrUpdateAction();
            }
            else
            {
                MessageBox.Show("ID " + this.textBoxID.Text + " không hợp lệ.");
            }
        }

        public void SaveOrUpdateAction()
        {
            // Edit mode: originalCharacterId is set
            // Add mode: originalCharacterId is null
            if(originalCharacterId == null)
            {
                // Add new character
                if(IsNotExist())
                {
                    character = new Character();
                    character.ID = this.textBoxID.Text;
                    character.Link = this.textBoxLink.Text;
                    character.Group = this.textBoxGroup.Text;
                    try
                    {
                        CharacterList.InsertCharacter(character);
                        this.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Thêm mới character " + character.ID + " không thành công.");
                    }
                }
                else
                {
                    MessageBox.Show("Character ID '" + this.textBoxID.Text + "' đã tồn tại. Vui lòng chọn ID khác.");
                }
            }
            else
            {
                // Edit existing character - use original ID
                character = CharacterList.GetCharacter(originalCharacterId);
                if (character != null)
                {
                    character.Link = this.textBoxLink.Text;
                    character.Group = this.textBoxGroup.Text;
                    try
                    {
                        CharacterList.UpdateCharacter(character);
                        Helper.saveSettingsToXML(character);
                        this.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Cập nhật character " + character.ID + " không thành công.");
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy character để cập nhật.");
                }
            }
        }

        private bool IsNotExist()
        {
            character = CharacterList.GetCharacter(this.textBoxID.Text);
            if (character == null)
            {
                return true;
            }
            return false;

        }

        private bool isValidate()
        {
            if (textBoxID.Text != string.Empty)
            {
                return true;
            }
            return false;
        }

        public void loadData()
        {
            character = CharacterList.GetCharacter(item);
            if (character != null)
            {
                // Set edit mode
                originalCharacterId = character.ID;

                // Populate form
                this.buttonAddNewCharacter.Text = "Cập nhật";
                this.textBoxID.Text = character.ID;
                this.textBoxID.Enabled = false; // Disable ID field during edit - ID is immutable
                this.textBoxLink.Text = character.Link;
                this.textBoxGroup.Text = character.Group;
            }
        }
    }
}
