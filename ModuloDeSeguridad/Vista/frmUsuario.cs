﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModuloDeSeguridad.Vista
{
    public partial class frmUsuario : Form, Logica.Interfaces.ISesionObserver
    {
        private Accion accion;
        private Logica.UsuarioBL usuarioBL;
        private Modelo.Usuario user;
        private List<Modelo.Grupo> gruposAll;
        private List<CheckBox> checkBoxes;
        public frmUsuario()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Logica.SesionBL.ObtenerInstancia().Suscribir(this);
            accion = Accion.Alta;
            user = new Modelo.Usuario();
            usuarioBL = new Logica.UsuarioBL();
            gruposAll = usuarioBL.ListarGrupos();
            foreach (var cbGrupo in ListarCheckBoxesGrupos(gruposAll))
            {
                flpGrupos.Controls.Add(cbGrupo);
            }
        }
        public frmUsuario(Accion miAccion, int id)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Logica.SesionBL.ObtenerInstancia().Suscribir(this);
            usuarioBL = new Logica.UsuarioBL();
            accion = miAccion;
            user = usuarioBL.Consultar(id);
            txtUsername.Text = user.Username;
            txtContrasena.Text = user.Password;
            txtConfirmarContrasena.Text = user.Password;
            txtContrasena.Enabled = false;
            txtConfirmarContrasena.Enabled = false;
            txtContrasena.Visible = false;
            txtConfirmarContrasena.Visible = false;
            lblContrasena.Visible = false;
            lblRepetirContrasena.Visible = false;

            btnAceptar.Location = new Point(283, 397);
            btnCancelar.Location = new Point(434, 397);
            this.Size = new Size(880, 504);

            txtEmail.Text = user.Email;
            txtNombre.Text = user.Nombre;
            txtApellido.Text = user.Apellido;
            gruposAll = usuarioBL.ListarGrupos();
            foreach (var cbGrupo in ListarCheckBoxesGrupos(gruposAll))
            {
                flpGrupos.Controls.Add(cbGrupo);
            }
            btnAceptar.Enabled = Accion.Consulta == accion ? false : true;
        }

        private List<CheckBox> ListarCheckBoxesGrupos(List<Modelo.Grupo> grupos)
        {
            checkBoxes = new List<CheckBox>();
            foreach (var grupo in grupos)
            {
                var checkb = new CheckBox();
                checkb.AutoSize = true;
                checkb.Name = grupo.ID.ToString();
                checkb.Text = grupo.Descripcion;
                if (accion != Accion.Alta)
                {
                    checkb.Checked = user.Grupos.Find(x => x.ID == grupo.ID) != null ? true : false;
                }
                else
                {
                    checkb.Checked = false;
                }
                checkb.Width = 140;
                checkBoxes.Add(checkb);
            }
            return checkBoxes;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Actualizar(false);
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtUsername.Text)   ||
                String.IsNullOrWhiteSpace(txtEmail.Text) ||
                String.IsNullOrWhiteSpace(txtNombre.Text) ||
                String.IsNullOrWhiteSpace(txtApellido.Text) ||
                String.IsNullOrWhiteSpace(txtContrasena.Text) ||
                String.IsNullOrWhiteSpace(txtConfirmarContrasena.Text))
            {
                MessageBox.Show("Debe completar todos los campos");
                return;
            }
            if (txtContrasena.Text != txtConfirmarContrasena.Text)
            {
                MessageBox.Show("Las contraseñas deben ser iguales");
                return;
            }
            try//validar email
            {
                var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                if (addr.Address != txtEmail.Text)
                {
                    MessageBox.Show("El email no es válido");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("El email no es válido");
                return;
            }
            user.Grupos = new List<Modelo.Grupo>();
            foreach (var cb in checkBoxes)
            {
                if (cb.Checked)
                {
                    user.Grupos.Add(gruposAll.Find(x => x.ID.ToString() == cb.Name));
                }
            }
            user.Username = txtUsername.Text;
            user.Email = txtEmail.Text;
            user.Nombre = txtNombre.Text;
            user.Apellido = txtApellido.Text;
            user.Estado = true;
            try
            {
                if (accion == Accion.Alta)
                {
                    user.Password = txtContrasena.Text;
                    usuarioBL.Insertar(user, Modelo.Sesion.ObtenerInstancia().Usuario.ID);
                }
                else
                {
                    usuarioBL.Modificar(user, Modelo.Sesion.ObtenerInstancia().Usuario.ID,true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            Logica.SesionBL.ObtenerInstancia().Desuscribir(this);
            this.DialogResult = DialogResult.OK;
        }

        public void Actualizar(bool isFirst)
        {
            if (isFirst)
            {
                MessageBox.Show("Su sesión se cerrará automaticamente");
            }
            Logica.SesionBL.ObtenerInstancia().Desuscribir(this);
            this.Dispose();
        }
    }
}
