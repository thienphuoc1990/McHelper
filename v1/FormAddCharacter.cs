using AutoVPT.Objects;
using System;
using System.Windows.Forms;

namespace AutoVPT
{
    public partial class FormAddCharacter : Form
    {
        Character character;
        public string item;
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
            if(IsNotExist())
            {
                character = new Character();
                character.ID = this.textBoxID.Text;
                character.Link = this.textBoxLink.Text;
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
                character.Link = this.textBoxLink.Text;
                try
                {
                    CharacterList.UpdateCharacter(character);
                    this.Close();
                }
                catch
                { 
                    MessageBox.Show("Cập nhật character " + character.ID + " không thành công.");
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
                this.buttonAddNewCharacter.Text = "Cập nhật";
                this.textBoxID.Text = character.ID;
                this.textBoxLink.Text = character.Link;
            }
        }
    }
}
