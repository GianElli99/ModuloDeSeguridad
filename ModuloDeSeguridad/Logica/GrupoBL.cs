﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloDeSeguridad.Logica
{
    public class GrupoBL
    {
        private Datos.Interfaces.IGrupoDAO grupoDAO;
        private Datos.Interfaces.IUsuarioDAO usuarioDAO;
        public GrupoBL()
        {
            grupoDAO = new Datos.DAO.GrupoDAO_SqlServer();
            usuarioDAO = new Datos.DAO.UsuarioDAO_SqlServer();
        }

        public List<Modelo.Accion> ListarAccionesDisponibles(int userId,int vistaId)
        {
            try
            {
                return usuarioDAO.ListarAccionesDisponibles(userId, vistaId);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void Insertar(Modelo.Grupo group, List<Modelo.Permiso> permisos)
        {
            try
            {
                if (DescripcionCodigoDisponible(group.Descripcion, group.Codigo, null))
                {
                    grupoDAO.Insertar(group, permisos);
                }
                else
                {
                    throw new Exception("La descripción o el código no están disponible");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void Modificar(Modelo.Grupo group, List<Modelo.Permiso> permisos)
        {
            try
            {
                if (DescripcionCodigoDisponible(group.Descripcion,group.Codigo,group.ID.ToString()))
                {
                    grupoDAO.Modificar(group, permisos);
                }
                else
                {
                    throw new Exception("La descripción o el código no están disponible");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void Eliminar(int id)
        {
            try
            {
                int users = grupoDAO.CantidadUsuarios(id);
                if (users == 0)
                {
                    grupoDAO.Eliminar(id);
                }
                else
                {
                    throw new Exception("Este grupo posee " + users + " usuarios, no puede ser eliminado.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Modelo.Grupo Consultar(int id)
        {
            try
            {
                return grupoDAO.Consultar(id);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<Modelo.Grupo> Listar()
        {
            try
            {
                return grupoDAO.Listar();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<Modelo.Grupo> Listar(List<Modelo.Grupo> grupos, string filtro)
        {
            try
            {
                return grupos.FindAll(x => x.Descripcion.ToUpper().Contains(filtro.ToUpper()) || x.Codigo.ToUpper().Contains(filtro.ToUpper()));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<Modelo.Permiso> ListarPermisos()//este se usa en el alta
        {
            try
            {
                List<int[]> permisosID = grupoDAO.ListarIDPermisos();
                var vistas = grupoDAO.ListarVistas();
                var acciones = grupoDAO.ListarAcciones();
                List<Modelo.Permiso> permisos = new List<Modelo.Permiso>();
                foreach (var permisoID in permisosID)
                {
                    var permiso = new Modelo.Permiso();
                    foreach (var accion in acciones)// revisar foreach y findT de la lista
                    {
                        if (accion.ID == permisoID[0])
                        {
                            permiso.Accion = accion;
                            break;
                        }
                    }
                    foreach (var vista in vistas)// revisar foreach y findT de la lista
                    {
                        if (vista.ID == permisoID[1])
                        {
                            permiso.Vista = vista;
                            break;
                        }
                    }
                    permiso.TienePermiso = false;
                    permisos.Add(permiso);
                }
                return permisos;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<Modelo.Permiso> ListarPermisos(int id)// se usa en mod y consulta
        {
            try
            {
                var grupo = grupoDAO.Consultar(id);
                var vistas = grupoDAO.ListarVistas();
                var acciones = grupoDAO.ListarAcciones();
                var permisosID = grupoDAO.ListarIDPermisos(id);
                var permisos = new List<Modelo.Permiso>();
                    foreach (var permisoID in permisosID)// validar que tenga datos
                    {
                        var permiso = new Modelo.Permiso();
                        permiso.ID = permisoID[0];
                        permiso.Grupo = grupo;
                        foreach (var vista in vistas)
                        {
                            if (vista.ID == permisoID[1])
                            {
                                permiso.Vista = vista;
                                break;
                            }
                        }
                        foreach (var accion in acciones)
                        {
                            if (accion.ID == permisoID[2])
                            {
                                permiso.Accion = accion;
                                break;
                            }
                        }
                        permiso.TienePermiso = permisoID[3] == 1 ? true : false;
                        permisos.Add(permiso);
                    }
                    return permisos;
            }
            catch (Exception)
            {
                throw new Exception("Ha ocurrido un error, contacte a un administrador");
            }
        }
        private bool DescripcionCodigoDisponible(string descripción, string codigo, string id)
        {
            try
            {
                return grupoDAO.DescripcionCodigoDisponible(descripción, codigo, id);  
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
