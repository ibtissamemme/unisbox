using System.Windows.Forms;

namespace SandBox.Periph_Sign_Screen
{
    public partial class Form_Sign_Screen_clone : Form
    {

        private Form_Sign_Screen _FrParent;

        public Form_Sign_Screen_clone(Form_Sign_Screen FrParent)
        {
            InitializeComponent();

            _FrParent = FrParent;
        }

        private void btn_Close_Click(object sender, System.EventArgs e)
        {
            this.Close();
            _FrParent.Disactive();
        }
    }
}
