﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FrbaHotel.Utilidades;
using System.Data.SqlClient;
using FrbaHotel.Entidades;

namespace FrbaHotel.AbmRol
{
    public partial class DatosRol : Form
    {
        private bool modificacion;
        private List<Funcionalidad> funcionalidades = new List<Funcionalidad>();
        private int idRolModif;

        public DatosRol()
        {
            InitializeComponent();
            modificacion = false;
            cargarFuncionalidades();
            checkBoxHabilitado.Checked = true;
        }

        public DatosRol(String nombre)
        {
            InitializeComponent();
            modificacion = true;
            textBoxNombreRol.Text = nombre;
            idRolModif = buscarIdRol();
            cargarFuncionalidades();
            cargarRol();
        }

        private void cargarFuncionalidades()
        {
            DB.ejecutarReader(
                "SELECT Descripcion, Id_Funcion " +
                "FROM LA_QUERY_DE_PAPEL.Funcionalidad",
            cargarCheckBoxs);
        }

        public void cargarCheckBoxs(SqlDataReader reader)
        {            
            funcionalidades.Add(new Funcionalidad(reader.GetString(0), reader.GetInt32(1).ToString()));
            checkedListBoxFuncionalidades.Items.Add(reader.GetString(0));
        }

        private void buttonAceptar_Click(object sender, EventArgs e)
        {
            errorProviderDatos.Clear();
            validarDatos();
            if (Validaciones.errorProviderConError(errorProviderDatos, Controls))
                return;

            if (modificacion)
                atenderModificacion();
            else
                atenderAlta();
        }

        private void validarDatos()
        {
            if (textBoxNombreRol.Text == "")
                errorProviderDatos.SetError(textBoxNombreRol, "No debe estar vacio");

            if (checkedListBoxFuncionalidades.CheckedItems.Count == 0)
                errorProviderDatos.SetError(checkedListBoxFuncionalidades, "Debe elegir al menos una funcionalidad");
        }

        private void buttonLimpiar_Click(object sender, EventArgs e)
        {
            Limpiador.LimpiarTextBox(this.Controls);
            checkedListBoxFuncionalidades.ClearSelected();
        }

        private int buscarIdRol()
        {
            return (int)DB.correrQueryEscalar(
                "SELECT Id_Rol " +
                "FROM LA_QUERY_DE_PAPEL.Rol " +
                "WHERE Nombre = '" + textBoxNombreRol.Text + "'");
        }

        private int checkBoxHabilitadoInt()
        {
            return checkBoxHabilitado.Checked ? 1 : 0;
        }

        /////////////////////ALTA///////////////////////////
        private void atenderAlta()
        {
            insertarRol();

            insertarFuncionalidadxRol();

            MessageBox.Show("Se creo el rol");
        }

        private int insertarRol()
        {
            return DB.correrQuery(
                    "INSERT INTO LA_QUERY_DE_PAPEL.Rol (Nombre, Habilitado) " +
                    "VALUES ('" + textBoxNombreRol.Text + "', " + checkBoxHabilitadoInt().ToString() + ")");
        }

        private void insertarFuncionalidadxRol()
        {
            int idRol = buscarIdRol();
            string id;

            foreach(string desc in checkedListBoxFuncionalidades.CheckedItems)
            {
                id = funcionalidades.Find(funcionalidad => funcionalidad.descripcion == desc).id;

                DB.correrQuery(
                    "INSERT INTO LA_QUERY_DE_PAPEL.FuncionalidadxRol (Id_Rol, Id_Funcion) " +
                    "VALUES (" + idRol.ToString() + ", " + id + ")");
            }
        }

        /////////////////////MODIFICACION///////////////////////////
        private void cargarRol()
        {
            //int idRol = buscarIdRol();
            checkBoxHabilitado.Checked = (bool)DB.correrQueryEscalar(
                "SELECT Habilitado " +
                "FROM LA_QUERY_DE_PAPEL.Rol " +
                    "WHERE Nombre = '" + textBoxNombreRol.Text + "'");

            DB.ejecutarReader(
                "SELECT Id_Funcion " +
                "FROM LA_QUERY_DE_PAPEL.FuncionalidadxRol " +
                    "WHERE Id_rol = " + idRolModif.ToString(), cargarFuncionalidad);
        }

        public void cargarFuncionalidad(SqlDataReader reader)
        {
            string descripcion = funcionalidades.Find(funcionalidad => funcionalidad.id == reader.GetInt32(0).ToString()).descripcion;

            int indice = checkedListBoxFuncionalidades.Items.IndexOf(descripcion);

            checkedListBoxFuncionalidades.SetItemChecked(indice, true);
        }

        private void atenderModificacion()
        {
            //actualizo rol
            DB.correrQuery(
                "UPDATE LA_QUERY_DE_PAPEL.Rol " +
                "SET Nombre = '" + textBoxNombreRol.Text + "', " +
                    "Habilitado = " + checkBoxHabilitadoInt().ToString() +
                "WHERE Id_Rol = " + idRolModif.ToString());

            //borro funcionalidades anteriores
            DB.correrQuery(
                "DELETE FROM LA_QUERY_DE_PAPEL.FuncionalidadxRol " +
                "WHERE Id_Rol = " + idRolModif.ToString());

            insertarFuncionalidadxRol();
        } 
    }
}