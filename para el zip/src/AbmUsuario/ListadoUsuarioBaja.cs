﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FrbaHotel.Utilidades;
using FrbaHotel.Entidades;
using System.Windows.Forms;

namespace FrbaHotel.AbmUsuario
{
    class ListadoUsuarioBaja : ListadoPersona
    {
        public ListadoUsuarioBaja(Usuario usuario) : base(usuario) { }

        protected override void cargarTabla(string tipoDoc)
        {
            dataGridViewPersonas.DataSource = DB.ejecutarQueryDeTabla(
                "SELECT u.Id_Usuario, Username, Password, Id_Rol, Nombre, Apellido, Tipo_Documento, Nro_Documento, Mail, Telefono, Direccion, Fecha_Nacimiento, Habilitado " +
                    "FROM LA_QUERY_DE_PAPEL.usuarios u " +
                    "JOIN LA_QUERY_DE_PAPEL.UsuarioxHotel uh " +
                        "ON u.Id_Usuario = uh.Id_Usuario " +
                        "WHERE Nombre LIKE @nombre " +
                            "AND Apellido LIKE @apellido " +
                            "AND Tipo_Documento LIKE @tipoDocumento " +
                            "AND Nro_Documento LIKE @nroDocumento " +
                            "AND Mail LIKE @mail " +
                            "AND Id_Hotel = @idHotel " +
                            "AND Habilitado = 1",
                "nombre", "%" + textBoxNombre.Text + "%", "apellido", "%" + textBoxApellido.Text + "%", "tipoDocumento", "%" + tipoDoc + "%",
                "nroDocumento", "%" + maskedTextBoxNroDoc.Text + "%", "mail", "%" + textBoxMail.Text + "%", "idHotel", usuario.idHotel); 
        }

        protected override string textoBoton()
        {
            return "Eliminar";
        }

        protected override void accionBoton(DataGridViewCellEventArgs e)
        {
            DB.ejecutarQuery(
                "DELETE FROM LA_QUERY_DE_PAPEL.usuarios " +
                "WHERE Username = @username",
                "username", dataGridViewPersonas.CurrentRow.Cells["Username"].Value.ToString());

            llenarTabla();
            MessageBox.Show("Usuario eliminado");
        }
    }
}
