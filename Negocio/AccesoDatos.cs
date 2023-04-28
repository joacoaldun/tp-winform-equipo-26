﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Negocio
{
    public class AccesoDatos
    {

        private SqlConnection conexion;
        private SqlCommand comando;
        private SqlDataReader lector;
        public SqlDataReader Lector
        {
            get { return lector; }
        }

        //CONEXION A BD
        public AccesoDatos()
        {   
           conexion = new SqlConnection("server=.\\SQLExpress; database=CATALOGO_P3_DB; integrated security=true;");
           comando = new SqlCommand();

        }

        //SETEAR CONSULTA
        public void setearConsulta(string consulta)
        {

            comando.CommandType = System.Data.CommandType.Text;
            comando.CommandText = consulta;
        }

        //EJECUTAR CONSULTA
        public void ejecutarConsulta()
        {
            comando.Connection = conexion;

            try
            {
                conexion.Open();
                //EJECUTAR LECTURA
                lector = comando.ExecuteReader();


            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        //CERRAR CONEXION
        public void cerrarConexion()
        {

            if (lector != null)
                lector.Close();

            conexion.Close();


        }



    }
}
