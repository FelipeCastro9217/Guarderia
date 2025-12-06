// Logica/Logica_Usuarios.cs
using Guarderia.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Guarderia.Logica
{
    public class Logica_Usuarios
    {
        private readonly string _connectionString;

        public Logica_Usuarios(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Usuario? EncontrarUsuario(string correo, string clave)
        {
            Usuario? usuario = null;

            using (SqlConnection conexion = new SqlConnection(_connectionString))
            {
                string consulta = "SELECT IdUsuario, Nombres, Correo, Clave, IdRol FROM Usuarios WHERE Correo = @pcorreo AND Clave = @pclave";
                SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.AddWithValue("@pcorreo", correo);
                comando.Parameters.AddWithValue("@pclave", clave);
                comando.CommandType = CommandType.Text;

                try
                {
                    conexion.Open();
                    using (SqlDataReader datos = comando.ExecuteReader())
                    {
                        if (datos.Read())
                        {
                            usuario = new Usuario
                            {
                                IdUsuario = (int)datos["IdUsuario"],
                                Nombres = datos["Nombres"].ToString() ?? "",
                                Correo = datos["Correo"].ToString() ?? "",
                                Clave = datos["Clave"].ToString() ?? "",
                                IdRol = (int)datos["IdRol"]
                            };
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return usuario;
        }
    }
}
