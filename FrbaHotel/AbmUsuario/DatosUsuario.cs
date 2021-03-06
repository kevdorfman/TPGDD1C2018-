﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FrbaHotel.Entidades;
using System.Data.SqlClient;
using FrbaHotel.Utilidades;

namespace FrbaHotel.AbmUsuario
{
    public partial class DatosUsuario : Form
    {
        private bool alta;
        private List<Hotel> hoteles = new List<Hotel>();
        private int idUsuarioAModificar;

        public DatosUsuario()
        {
            InitializeComponent();
            alta = true;
            cargarHoteles();
            cargarRoles();
            cargarTiposDoc();
        }

        public DatosUsuario(DataGridViewRow filaSeleccionada)
        {
            InitializeComponent();
            alta = false;
            cargarHoteles();
            cargarRoles();
            cargarTiposDoc();
            cargarUsuario(filaSeleccionada);
            pictureBoxWarning.Image = SystemIcons.Warning.ToBitmap();
            toolTipDatosUsuario.SetToolTip(pictureBoxWarning, "Si no se deasea cambiar su password \ndeje el campo en blanco");
        }

        private void cargarHoteles()
        {
            DB.ejecutarReader(
                "SELECT Nombre, Id_Hotel " +
                "FROM LA_QUERY_DE_PAPEL.Hotel",
            cargarCheckBoxs);
        }

        public void cargarCheckBoxs(SqlDataReader reader)
        {
            while (reader.Read())
            {
                hoteles.Add(new Hotel(reader.GetString(0), reader.GetInt32(1).ToString()));
                checkedListBoxHoteles.Items.Add(reader.GetString(0));
            }
        }

        private void cargarRoles()
        {
            DB.ejecutarReader(
                "SELECT Nombre " +
                "FROM LA_QUERY_DE_PAPEL.Rol", 
            cargarComboBox);
        }

        public void cargarComboBox(SqlDataReader reader)
        {
            while (reader.Read())
            {
                comboBoxRoles.Items.Add(reader.GetString(0));
            }
        }

        private void buttonAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                errorProviderDatosUsuario.Clear();
                validarDatos();
                if (Validaciones.errorProviderConError(errorProviderDatosUsuario, Controls))
                    return;

                if (alta)
                    atenderAlta();
                else
                    atenderModificacion();
            }
            catch (SqlException) { }
        }

        private void validarDatos()
        {
            Validaciones.validarControles(errorProviderDatosUsuario, Controls);
            Validaciones.validarFechasPosteriores(errorProviderDatosUsuario, Controls);
            if (!alta)
                errorProviderDatosUsuario.SetError(textBoxPassword, "");
        }

        /////////////////////ALTA///////////////////////////
        private void atenderAlta()
        {
            insertarUsuario();

            insertUsuarioxHotel();

            MessageBox.Show("Se creo el usuario");
        }

        private void insertarUsuario()
        {
            int idRol = DB.buscarIdRol(comboBoxRoles.SelectedItem.ToString());
            DB.ejecutarQuery(
                    "INSERT INTO LA_QUERY_DE_PAPEL.usuarios (Username, Password , Id_Rol, Nombre, Apellido, Tipo_Documento, Nro_Documento, Mail, Telefono, Direccion, Fecha_Nacimiento, Habilitado) " +
                    "VALUES (@username, @password, @idRol, @nombre, @apellido, @tipoDocumento, @nroDocumento, @mail, @telefono, @direccion, @fechaNacimiento, @habilitado)",
                    "username", textBoxUsername.Text, "password", Usuario.encriptar(textBoxPassword.Text), "idRol", idRol,
                    "nombre", textBoxNombre.Text, "apellido", textBoxApellido.Text, "tipoDocumento", comboBoxTipoDoc.SelectedItem, "nroDocumento", textBoxNroDocumento.Text,
                    "mail", textBoxMail.Text, "telefono", textBoxTelefono.Text, "direccion", textBoxDireccion.Text, "fechaNacimiento", dateTimePickerFechaNac.Value,
                    "habilitado", checkBoxHabilitado.Checked);
        }

        private void insertUsuarioxHotel()
        {
            int idUsuario = DB.buscarIdUsuario(textBoxUsername.Text);
            string id;

            foreach (string desc in checkedListBoxHoteles.CheckedItems)
            {
                id = hoteles.Find(hotel => hotel.nombre == desc).id;

                DB.ejecutarQuery(
                    "INSERT INTO LA_QUERY_DE_PAPEL.UsuarioxHotel (Id_Hotel, Id_Usuario) " +
                    "VALUES (@idHotel, @idUsuario)",
                    "idHotel", id, "idUsuario", idUsuario);
            }
        }

        /////////////////////MODIFICACION///////////////////////////
        private void atenderModificacion()
        {
            int idRol = DB.buscarIdRol(comboBoxRoles.SelectedItem.ToString());
            
            DB.ejecutarQuery(
                "UPDATE LA_QUERY_DE_PAPEL.usuarios " +
                querySet() + 
                "WHERE Id_Usuario = @idUsuario",
                "username", textBoxUsername.Text, "password", Usuario.encriptar(textBoxPassword.Text), "idRol", idRol,
                "nombre", textBoxNombre.Text, "apellido", textBoxApellido.Text, "tipoDoc", comboBoxTipoDoc.SelectedItem, "nroDoc", textBoxNroDocumento.Text,
                "mail", textBoxMail.Text, "telefono", textBoxTelefono.Text, "direccion", textBoxDireccion.Text, "fechaNac", dateTimePickerFechaNac.Value,
                "habilitado", checkBoxHabilitado.Checked, "idUsuario", idUsuarioAModificar);

            DB.ejecutarQuery(
                "DELETE FROM LA_QUERY_DE_PAPEL.UsuarioxHotel " +
                "WHERE Id_Usuario = @idUsuario",
                "idUsuario", idUsuarioAModificar);

            insertUsuarioxHotel();

            MessageBox.Show("Se modifico el rol");
        }

        private String querySet()
        {
            if (textBoxPassword.Text == "")
            {
                return "SET Username = @username, Id_Rol = @idRol, Nombre = @nombre, Apellido = @apellido, Tipo_Documento = @tipoDoc, " +
                "Nro_Documento = @nroDoc, Mail = @mail, Telefono = @telefono, Direccion = @direccion, Fecha_Nacimiento = @fechaNac, Habilitado = @habilitado ";
            }
            else
                 return "SET Username = @username, Password = @password, Id_Rol = @idRol, Nombre = @nombre, Apellido = @apellido, Tipo_Documento = @tipoDoc, " +
                 "Nro_Documento = @nroDoc, Mail = @mail, Telefono = @telefono, Direccion = @direccion, Fecha_Nacimiento = @fechaNac, Habilitado = @habilitado ";
        }

        private void cargarUsuario(DataGridViewRow filaSeleccionada)
        {
            textBoxUsername.Text = filaSeleccionada.Cells["Username"].Value.ToString();
            textBoxNombre.Text = filaSeleccionada.Cells["Nombre"].Value.ToString();
            textBoxApellido.Text = filaSeleccionada.Cells["Apellido"].Value.ToString();
            comboBoxTipoDoc.SelectedIndex = comboBoxTipoDoc.Items.IndexOf(filaSeleccionada.Cells["Tipo_Documento"].Value.ToString());
            textBoxNroDocumento.Text = filaSeleccionada.Cells["Nro_Documento"].Value.ToString();
            textBoxMail.Text = filaSeleccionada.Cells["Mail"].Value.ToString();
            textBoxTelefono.Text = filaSeleccionada.Cells["Telefono"].Value.ToString();
            textBoxDireccion.Text = filaSeleccionada.Cells["Direccion"].Value.ToString();
            dateTimePickerFechaNac.Value = Convert.ToDateTime(filaSeleccionada.Cells["Fecha_Nacimiento"].Value.ToString());
            checkBoxHabilitado.Checked = (bool)filaSeleccionada.Cells["Habilitado"].Value;
            idUsuarioAModificar = (int)filaSeleccionada.Cells["Id_Usuario"].Value;

            string nombreRol = DB.ejecutarQueryEscalar(
                "SELECT Nombre " +
                "FROM LA_QUERY_DE_PAPEL.Rol " +
                "WHERE Id_Rol = @idRol",
                "idRol", (int)filaSeleccionada.Cells["Id_Rol"].Value).ToString();

            comboBoxRoles.SelectedIndex = comboBoxRoles.Items.IndexOf(nombreRol);

            cargarHotelesDondeTrabaja();
        }

        private void cargarHotelesDondeTrabaja()
        {
            DB.ejecutarReader(
                "SELECT Id_Hotel " +
                "FROM LA_QUERY_DE_PAPEL.UsuarioxHotel " +
                    "WHERE Id_Usuario = @idUsuario",
                cargarHotel, "idUsuario", idUsuarioAModificar);
        }

        public void cargarHotel(SqlDataReader reader)
        {
            while (reader.Read())
            {
                string nombre = hoteles.Find(hotel => hotel.id == reader.GetInt32(0).ToString()).nombre;

                int indice = checkedListBoxHoteles.Items.IndexOf(nombre);

                checkedListBoxHoteles.SetItemChecked(indice, true);
            }
        }

        private void buttonLimpiar_Click(object sender, EventArgs e)
        {
            Limpiador.limpiarControles(Controls);
        }

        private void cargarTiposDoc()
        {
            DB.ejecutarReader(
                "SELECT distinct(Tipo_Documento) " +
                "FROM LA_QUERY_DE_PAPEL.Persona",
                cargarTipoDoc);
        }

        public void cargarTipoDoc(SqlDataReader reader)
        {
            while (reader.Read())
            {
                comboBoxTipoDoc.Items.Add(reader.GetString(0));
            }
        }
    }
}
