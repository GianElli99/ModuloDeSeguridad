﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModuloDeSeguridad.Modelo;
using System.Data.SqlClient;

namespace ModuloDeSeguridad.Datos.DAO
{
    public class UsuarioDAO_SqlServer : ConexionDB,Interfaces.IUsuarioDAO
    {
        public void CambiarContrasena(string pass, int userId, int editorId)
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Cambiar Contraseña");

                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = $"INSERT INTO usuarios_auditorias SELECT * FROM usuarios WHERE usuarios.id = {userId}";
                    command.ExecuteNonQuery();


                    command.CommandText = $"UPDATE usuarios SET contrasena=@contrasena, editor_id={editorId}, edicion_fecha=@edicion_fecha, edicion_accion='M' WHERE id = {userId}";
                    command.Parameters.AddWithValue("@contrasena", pass);
                    command.Parameters.AddWithValue("@edicion_fecha", DateTime.Now);
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    return;
                }
                catch (Exception)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {

                        throw ex2;
                    }
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public Usuario Consultar(int id)// traigo usuario y sus grupos
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Consulta Usuario");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = $"SELECT TOP 1 * from usuarios WHERE id = {id};SELECT grupos.id, grupos.codigo, grupos.descripcion,grupos.estado FROM grupos INNER JOIN usuarios_grupos ON grupos.id = usuarios_grupos.grupo_id WHERE usuarios_grupos.usuario_id = {id}";
                    transaction.Commit();
                    using (SqlDataReader response = command.ExecuteReader())
                    {
                        var usuario = new Usuario();
                        if (response.Read())
                        {
                            usuario.ID = response.GetInt32(0);
                            usuario.Username = response.GetString(1);
                            usuario.Password = response.GetString(2);
                            usuario.Email = response.GetString(3);
                            usuario.Nombre = response.GetString(4);
                            usuario.Apellido = response.GetString(5);
                            usuario.Estado = response.GetBoolean(6);
                        }
                        response.NextResult();
                        while (response.Read())
                        {
                            var grupo = new Grupo();
                            grupo.ID = response.GetInt32(0);
                            grupo.Codigo = response.GetString(1);
                            grupo.Descripcion = response.GetString(2);
                            grupo.Estado = response.GetBoolean(3);
                            usuario.Grupos.Add(grupo);
                        }
                        return usuario;
                    }
                    throw new Exception("No se ha podido encontrar el grupo");
                }
                catch (Exception ex)
                {
                        throw ex;
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public Usuario Consultar(string username, string email)
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Consulta Usuario");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = $"SELECT TOP 1 * from usuarios WHERE username = @username AND email =@email";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@email", email);
                    transaction.Commit();
                    using (SqlDataReader response = command.ExecuteReader())
                    {
                        var usuario = new Usuario();
                        if (response.Read())
                        {
                            usuario.ID = response.GetInt32(0);
                            usuario.Username = response.GetString(1);
                            usuario.Password = response.GetString(2);
                            usuario.Email = response.GetString(3);
                            usuario.Nombre = response.GetString(4);
                            usuario.Apellido = response.GetString(5);
                            usuario.Estado = response.GetBoolean(6);
                            return usuario;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public void Eliminar(int id, int idEditor)
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Eliminar usuario");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = $"INSERT INTO usuarios_auditorias SELECT * FROM usuarios WHERE usuarios.id = {id}";
                    command.ExecuteNonQuery();

                    command.CommandText = $"UPDATE usuarios SET estado=0, editor_id=@editor_id, edicion_fecha=@edicion_fecha, edicion_accion='B' WHERE id = {id}";
                    command.Parameters.AddWithValue("@editor_id", idEditor);
                    command.Parameters.AddWithValue("@edicion_fecha", DateTime.Now);
                    //command.CommandText = $"DELETE FROM usuarios_grupos WHERE usuario_id = {id}; DELETE FROM usuarios WHERE id = {id}";
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    return;
                }
                catch (Exception)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {

                        throw ex2;
                    }
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public void Insertar(Usuario t, int idEditor)
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Insertar usuario");

                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    int bitEstado = t.Estado ? 1 : 0;

                    command.CommandText = $"INSERT INTO usuarios VALUES(@username,@contrasena,@email,@nombre,@apelido,{bitEstado.ToString()},@editor_id,@edicion_fecha,'A');SELECT CAST(scope_identity() AS int)";
                    command.Parameters.AddWithValue("@username", t.Username);
                    command.Parameters.AddWithValue("@contrasena", t.Password);
                    command.Parameters.AddWithValue("@email", t.Email);
                    command.Parameters.AddWithValue("@nombre", t.Nombre);
                    command.Parameters.AddWithValue("@apelido", t.Apellido);
                    command.Parameters.AddWithValue("@editor_id", idEditor);
                    command.Parameters.AddWithValue("@edicion_fecha", DateTime.Now);
                    using (SqlDataReader response = command.ExecuteReader())
                    {
                        if (response.Read())
                        {
                            t.ID = response.GetInt32(0);
                        }
                    }
                    string querygrupos = $"INSERT INTO usuarios_grupos VALUES ";
                    foreach (var grupo in t.Grupos)
                    {
                        querygrupos += $"('{t.ID.ToString()}','{grupo.ID.ToString()}'),";
                    }
                    querygrupos = querygrupos.TrimEnd(',');
                    command.CommandText = querygrupos;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    return;
                }
                catch (Exception)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {

                        throw ex2;
                    }
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public List<Usuario> Listar()
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Listar usuarios");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = $"SELECT id,username,email,nombre,apellido,estado FROM usuarios WHERE estado=1";
                    transaction.Commit();
                    using (SqlDataReader response = command.ExecuteReader())
                    {
                        if (response.HasRows)
                        {
                            var usuarios = new List<Modelo.Usuario>();
                            while (response.Read())
                            {
                                var usuario = new Modelo.Usuario();

                                usuario.ID = response.GetInt32(0);
                                usuario.Username = response.GetString(1);
                                usuario.Email = response.GetString(2);
                                usuario.Nombre = response.GetString(3);
                                usuario.Apellido = response.GetString(4);
                                usuario.Estado = response.GetBoolean(5);
                                usuarios.Add(usuario);
                            }
                            return usuarios;
                        }
                    }
                }
                catch (Exception ex2)
                {
                        throw ex2;
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public List<Accion> ListarAccionesDisponibles(int idUser, int idVista)
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Listar vistas disponibles");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = $"SELECT DISTINCT acciones.id, acciones.tipo FROM permisos INNER JOIN acciones on permisos.accion_id = acciones.id WHERE permisos.grupo_id IN (SELECT grupo_id FROM usuarios_grupos INNER JOIN grupos ON grupos.id = usuarios_grupos.grupo_id WHERE usuario_id = {idUser} AND grupos.estado = 1) AND vista_id = {idVista} AND tiene_permiso = 1";
                    transaction.Commit();
                    using (SqlDataReader response = command.ExecuteReader())
                    {
                        if (response.HasRows)
                        {
                            var acciones = new List<Modelo.Accion>();
                            while (response.Read())
                            {
                                var accion = new Modelo.Accion();
                                accion.ID = response.GetInt32(0);
                                accion.Descripcion = response.GetString(1);
                                acciones.Add(accion);
                            }
                            return acciones;
                        }
                    }
                    throw new Exception("No se han podido encontrar acciones disponibles");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public List<Grupo> ListarGrupos(int id)//grupos del usuario
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Listar grupos del usuario");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = $"SELECT grupos.id, grupos.codigo, grupos.descripcion,grupos.estado FROM grupos INNER JOIN usuarios_grupos ON grupos.id = usuarios_grupos.grupo_id WHERE usuarios_grupos.usuario_id = {id} AND grupos.estado = 1";
                    transaction.Commit();
                    using (SqlDataReader response = command.ExecuteReader())
                    {
                        if (response.HasRows)
                        {
                            var grupos = new List<Modelo.Grupo>();
                            while (response.Read())
                            {
                                var grupo = new Modelo.Grupo();

                                grupo.ID = response.GetInt32(0);
                                grupo.Codigo = response.GetString(1);
                                grupo.Descripcion = response.GetString(2);
                                grupo.Estado = response.GetBoolean(3);
                                grupos.Add(grupo);
                            }
                            return grupos;
                        }
                    }
                }
                catch (Exception ex2)
                {
                    throw ex2;
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public List<Modelo.Vista> ListarVistasDisponibles(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Listar vistas disponibles");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = $"SELECT vista_id, vistas.nombre FROM permisos INNER JOIN vistas ON permisos.vista_id = vistas.id WHERE grupo_id IN(SELECT grupo_id FROM usuarios_grupos INNER JOIN grupos ON grupos.id = usuarios_grupos.grupo_id WHERE usuario_id = {id} AND grupos.estado = 1) GROUP BY vista_id, nombre HAVING SUM(CAST(tiene_permiso AS INT)) > 0";
                    transaction.Commit();
                    using (SqlDataReader response = command.ExecuteReader())
                    {
                        if (response.HasRows)
                        {
                            var vistas = new List<Modelo.Vista>();
                            while (response.Read())
                            {
                                var vista = new Modelo.Vista();
                                vista.ID = response.GetInt32(0);
                                vista.Descripcion = response.GetString(1);
                                vistas.Add(vista);
                            }
                            return vistas;
                        }        
                    }
                    throw new Exception("No se han podido encontrar vistas disponibles");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public void Modificar(Usuario t, int idEditor, bool modificaGrupo)
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Modificar usuario");

                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = $"INSERT INTO usuarios_auditorias SELECT * FROM usuarios WHERE usuarios.id = {t.ID}";
                    command.ExecuteNonQuery();

                    int bitEstado = t.Estado ? 1 : 0;

                    command.CommandText = $"UPDATE usuarios SET username=@username, email=@email, nombre=@nombre, apellido=@apelido, estado={bitEstado.ToString()}, editor_id=@editor_id, edicion_fecha=@edicion_fecha, edicion_accion='M' WHERE id = {t.ID.ToString()}";
                    command.Parameters.AddWithValue("@username", t.Username);
                    command.Parameters.AddWithValue("@email", t.Email);
                    command.Parameters.AddWithValue("@nombre", t.Nombre);
                    command.Parameters.AddWithValue("@apelido", t.Apellido);
                    command.Parameters.AddWithValue("@editor_id", idEditor);
                    command.Parameters.AddWithValue("@edicion_fecha", DateTime.Now);
                    command.ExecuteNonQuery();


                    if (modificaGrupo)
                    {
                        string querygrupos = $"DELETE FROM usuarios_grupos WHERE usuario_id = {t.ID};";


                        querygrupos += $"INSERT INTO usuarios_grupos VALUES ";
                        foreach (var grupo in t.Grupos)
                        {
                            querygrupos += $"('{t.ID.ToString()}','{grupo.ID.ToString()}'),";
                        }
                        querygrupos = querygrupos.TrimEnd(',');
                        command.CommandText = querygrupos;
                        command.ExecuteNonQuery();
                    }
                    
                    transaction.Commit();
                    return;
                }
                catch (Exception)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {

                        throw ex2;
                    }
                }
            }
            throw new Exception("Ha ocurrido un error");
        }

        public bool UsernameEmailDisponibles(string username, string email, string id)
        {
            using (SqlConnection connection = new SqlConnection(connectionSQL))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Validacion username y correo");

                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    string optionalQuery = id != null ? $" AND id <> {id}" : "";
                    command.CommandText = $"SELECT count(id) AS cantidad FROM usuarios WHERE (username = @username OR email = @email)" + optionalQuery;
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@email", email);
                    transaction.Commit();
                    using (SqlDataReader response = command.ExecuteReader())
                    {
                        if (response.Read())
                        {
                            return response.GetInt32(0) > 0 ? false : true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            throw new Exception("Ha ocurrido un error");
        }
    }
}
