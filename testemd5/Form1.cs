using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace testemd5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AtualizarGrid();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            AtualizarGrid();
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            CadastrarUsuario(txtLogin.Text, txtSenha.Text);
        }

        private void btnLogar_Click(object sender, EventArgs e)
        {
            ConsultaUsuario(txtLogin.Text, txtSenha.Text);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ExecuteNonQuery("delete from Usuario");
            AtualizarGrid();
        }

        public SqlParameter[] GerarUsuarioParams(string login, string senha)
        {
            senha = Encriptar(senha);

            return new SqlParameter[]
            {
                new SqlParameter("@login",login),
                new SqlParameter("@senha",senha)
            };
        }

        public void CadastrarUsuario(string login, string senha)
        {
            if (!CheckUsuario(login))
                if (ExecuteNonQuery($@"insert into Usuario (login,senha) values (@login, @senha); ", GerarUsuarioParams(login, senha)) > 0)
                {
                    MessageBox.Show("Usuário cadastrado com sucesso!");
                    AtualizarGrid();
                }
                else
                {
                    MessageBox.Show("Ooops! Não foi possível cadastrar o usuário, tente mais tarde!");
                }
            else
                MessageBox.Show("Ooops! Usuário já cadastrado!");
        }

        public void ConsultaUsuario(string login, string senha)
        {
            DataTable dt = ExecuteSelect($@"select * from Usuario where login = @login and senha = @senha; ", GerarUsuarioParams(login, senha));
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("Login efetuar com sucesso!");
            }
        }

        public bool CheckUsuario(string login)
        {
            return Convert.ToInt32(ExecuteScalar(@"select count(1) from Usuario where login = @login", new SqlParameter[] { new SqlParameter("@login", login) })) > 0;
        }

        public void AtualizarGrid()
        {
            dataGridView1.DataSource = ExecuteSelect($@"select * from Usuario");

        }

        public string Encriptar(string senha)
        {
            MD5 md5Hash = MD5.Create();

            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(senha));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }


        public SqlConnection CriarConexao()
        {
            var temp = Directory.GetCurrentDirectory();
            SqlConnection coon = new SqlConnection($@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True");




            return coon;
        }


        public int ExecuteNonQuery(string query, SqlParameter[] parametros = null)
        {
            using (var conn = CriarConexao())
            {
                try
                {
                    SqlCommand command = conn.CreateCommand();
                    conn.Open();
                    command.CommandText = query;
                    if (parametros != null)
                        command.Parameters.AddRange(parametros);
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parametros)
        {
            using (var conn = CriarConexao())
            {
                try
                {
                    SqlCommand command = conn.CreateCommand();
                    conn.Open();
                    command.CommandText = query;
                    if (parametros != null)
                        command.Parameters.AddRange(parametros);
                    return command.ExecuteScalar();
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public DataTable ExecuteSelect(string query, SqlParameter[] parametros = null)
        {
            using (var conn = CriarConexao())
            {
                try
                {
                    SqlCommand command = conn.CreateCommand();
                    conn.Open();
                    command.CommandText = query;
                    if (parametros != null)
                        command.Parameters.AddRange(parametros);

                    var result = command.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(result);
                    return dt;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        
    }
}
