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

namespace FrbaHotel.GenerarModificacionReserva
{
    public partial class ConfirmacionReserva : Form
    {
        private Reserva reserva;
        private Form form;

        public ConfirmacionReserva(Reserva reserva, DataTable tabla, Form form)
        {
            InitializeComponent();
            this.reserva = reserva;
            this.form = form;
            dateTimePickerDesde.Value = reserva.fechaInicio;
            dateTimePickerHasta.Value = reserva.fechaFin;
            textBoxTipoHab.Text = reserva.tipoHabitacion;
            textBoxTipoReg.Text = reserva.descRegimen;
            dataGridViewReserva.DataSource = tabla;
        }

        private void buttonCancelar_Click(object sender, EventArgs e)
        {
            form.Show();
            Close();
        }
    }
}
