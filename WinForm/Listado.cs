﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Negocio;
using Dominio;
using System.IO;
using System.Net;

namespace WinForm
{
    public partial class Listado : Form
    {
        private bool modificando = false;
        List<Articulo> listaArticulos = new List<Articulo>();
        string rutaImagen = "https://t3.ftcdn.net/jpg/02/48/42/64/240_F_248426448_NVKLywWqArG2ADUxDq6QprtIzsF82dMF.jpg";


        public Listado()
        {

            InitializeComponent();
            this.Text = string.Empty;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
        }
       
        
        //INICIALIZAMOS INDICE
        private int indiceImagenActual;
        private int imagenActual = 0;

        //LOAD DE LISTADO
        private void Listado_Load(object sender, EventArgs e)
        {
            cargar();
            cboCampo.Items.Add("Nombre");
            cboCampo.Items.Add("Marcas");
            cboCampo.Items.Add("Precio");
            cboCampo.Items.Add("Categorias");
            cboCampo.Items.Add("Descripcion");
            cboCampo.Items.Add("Codigo Articulo");
           
        }

        //CARGAMOS CBO FILTROS
        private void cboCampo_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboCriterio.Items.Clear();
            txtFiltro.Enabled = true;
            string opcion = cboCampo.SelectedItem.ToString();
            if (opcion == "Precio")
            {
                cargarCboCriterio("Mayor a", "Menor a", "Igual a");
            }
            else if (opcion == "Categorias")
            {
                txtFiltro.Enabled = false;
                cargarCboCategorias();
            }
            else if (opcion == "Marcas")
            {
                txtFiltro.Enabled = false;
                cargarCboMarcas();
            }
            else
            {
                cargarCboCriterio("Comienza con","Termina con","Contiene");
            }
        }

        private void cargarCboCategorias()
        {
            CategoriaNegocio negocio = new CategoriaNegocio();
            List<Categoria>categorias = new List<Categoria>();
            categorias=negocio.listar();

            foreach (Categoria categoria in categorias)
            {
                cboCriterio.Items.Add(categoria.NombreCategoria);
            }

        }

        private void cargarCboMarcas()
        {
            MarcaNegocio negocio = new MarcaNegocio();
            List<Marca> marcas = new List<Marca>();
            marcas = negocio.listar();

            foreach (Marca marca in marcas)
            {
                cboCriterio.Items.Add(marca.NombreMarca);
            }
        }


        private void cargarCboCriterio(string criterio1, string criterio2, string criterio3)
        {
            cboCriterio.Items.Add(criterio1);
            cboCriterio.Items.Add(criterio2);
            cboCriterio.Items.Add(criterio3);
        }

        //FILTRO AVANZADO

        private bool soloNumeros(string cadena)
        {
            foreach (char caracter in cadena)
            {
                if ((char.IsNumber(caracter)))
                    return true;
            }
            return false;
        }


        private bool validarFiltro()
        {
            if (cboCampo.SelectedIndex < 0 || cboCriterio.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar un campo y un criterio de búsqueda.", "Error de selección", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (cboCampo.SelectedItem.ToString() == "Precio")
            {
                if (string.IsNullOrEmpty(txtFiltro.Text))
                {
                    MessageBox.Show("Por favor ingresa un filtro para numéricos");
                    return false;
                }
                if (!soloNumeros(txtFiltro.Text))
                {
                    MessageBox.Show("Solo se aceptan números para filtrar un campo numerico");
                    return false;
                }
            }
            return true;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {


            ArticuloNegocio negocio = new ArticuloNegocio();
            try
            {   // Verificamos si los ComboBox no estan nulos.
                if (validarFiltro()) 
                {
                    string campo = cboCampo.SelectedItem.ToString();
                    string criterio = cboCriterio.SelectedItem.ToString();
                    string filtro = txtFiltro.Text;


                    if (cboCampo.SelectedItem.ToString()=="Marcas" || cboCampo.SelectedItem.ToString() == "Categorias")
                    {
                        filtro = criterio;
                    }

                    //Verificamoss si el filtro esta vacio
                    if (string.IsNullOrWhiteSpace(filtro))
                    {  // Cargar la lista completa de arts
                        cargar(); 
                    }
                    else
                    {   // Si el filtro no esta vacio, filtra
                        dgvListaArticulos.DataSource = negocio.filtrar(campo, criterio, filtro); 
                       
                    }
                }
                txtFiltro.Clear();
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            
        }

        

        


        //CARGAMOS IMAGEN EN PBX
        private void cargarImagen(string imagen)
        {
            try
            {
                
                pbxArticulo.Load(imagen);
            }
            catch (Exception)
            {
                pbxArticulo.Load("https://t3.ftcdn.net/jpg/02/48/42/64/240_F_248426448_NVKLywWqArG2ADUxDq6QprtIzsF82dMF.jpg");


            }

        }

        //CAMBIA DE IMAGEN SEGUN FILA
        
        private void dgvListaArticulos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvListaArticulos.CurrentRow != null)
            {
                int rowIndex = dgvListaArticulos.CurrentRow.Index;
                if (listaArticulos.Count > rowIndex && listaArticulos[rowIndex].imagenes.Count > 0)
                {
                    imagenActual = 0; // reiniciamos la imagen actual
                    string url = listaArticulos[rowIndex].imagenes[imagenActual];
                    string urlEscapada = Uri.EscapeUriString(url);
                    try
                    {
                        using (var webClient = new System.Net.WebClient())
                        {
                            var imagenDescargada = webClient.DownloadData(urlEscapada);
                            using (var stream = new MemoryStream(imagenDescargada))
                            {
                                pbxArticulo.Image = Image.FromStream(stream);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Construir la ruta de la imagen de respaldo 
                        // string rutaImagenRespaldo = Path.Combine(Application.StartupPath, "placeHolder.jpeg");
                        pbxArticulo.Load(rutaImagen);

                        // Cargar la imagen 
                        // pbxArticulo.Image = Image.FromFile(rutaImagenRespaldo);// Si ocurre un error al descargar la imagen, cargar una imagen de respaldo


                    }
                }
            }
        }



        //BOTONES
       


        private void btnIzquierda_Click(object sender, EventArgs e)
        {
            if (dgvListaArticulos.CurrentRow != null)
            {
                int rowIndex = dgvListaArticulos.CurrentRow.Index;
                if (listaArticulos.Count > rowIndex && listaArticulos[rowIndex].imagenes.Count > 0)
                {
                    imagenActual--;
                    if (imagenActual < 0)
                    {
                        imagenActual = listaArticulos[rowIndex].imagenes.Count - 1;
                    }
                    string url = listaArticulos[rowIndex].imagenes[imagenActual];
                    string urlEscapada = Uri.EscapeUriString(url);
                    try
                    {
                        using (var webClient = new System.Net.WebClient())
                        {
                            var imagenDescargada = webClient.DownloadData(urlEscapada);
                            using (var stream = new MemoryStream(imagenDescargada))
                            {
                                pbxArticulo.Image = Image.FromStream(stream);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Construir la ruta de la imagen de respaldo 
                        // string rutaImagenRespaldo = Path.Combine(Application.StartupPath, "placeHolder.jpeg");

                        pbxArticulo.Load(rutaImagen);

                        // Cargar la imagen 
                        // pbxArticulo.Image = Image.FromFile(rutaImagenRespaldo);// Si ocurre un error al descargar la imagen, cargar una imagen de respaldo
                    }


                }
            }
        }

        private void btnDerecha_Click(object sender, EventArgs e)
        {
            

            if (dgvListaArticulos.CurrentRow != null)
            {
                int rowIndex = dgvListaArticulos.CurrentRow.Index;

                if (listaArticulos.Count > rowIndex && listaArticulos[rowIndex].imagenes.Count > 0)
                {
                    imagenActual++;
                    if (imagenActual >= listaArticulos[rowIndex].imagenes.Count)
                    {
                        imagenActual = 0;
                        
                        
                    }
                    string url = listaArticulos[rowIndex].imagenes[imagenActual];
                    string urlEscapada = Uri.EscapeUriString(url);
                    try
                    {
                        using (var webClient = new System.Net.WebClient())
                        {
                            var imagenDescargada = webClient.DownloadData(urlEscapada);
                            using (var stream = new MemoryStream(imagenDescargada))
                            {
                                pbxArticulo.Image = Image.FromStream(stream);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Construir la ruta de la imagen de respaldo 
                        // string rutaImagenRespaldo = Path.Combine(Application.StartupPath, "placeHolder.jpeg");

                        pbxArticulo.Load(rutaImagen);

                        // Cargar la imagen 
                        // pbxArticulo.Image = Image.FromFile(rutaImagenRespaldo);// Si ocurre un error al descargar la imagen, cargar una imagen de respaldo
                    }

                    
                }
            }
        }

        private void dgvListaArticulos_DoubleClick(object sender, EventArgs e)
        {
            //MessageBox.Show("No hay nada por aqui...");
            Articulo seleccionado = new Articulo();
            seleccionado = (Articulo)dgvListaArticulos.CurrentRow.DataBoundItem;
            Descripcion descripcion = new Descripcion(seleccionado);
            descripcion.ShowDialog();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            modificando = false;
            AgregarArticulo agregarArticulo = new AgregarArticulo(modificando);
            agregarArticulo.ShowDialog();
            cargar();



            
        }

        public void cargar() {

            ArticuloNegocio negocio = new ArticuloNegocio();
            listaArticulos = negocio.listar();


            if (listaArticulos.Count > 0 && listaArticulos[0].imagenes.Count > 0)
            {
                string url = listaArticulos[0].imagenes[0];
                string urlEscapada = Uri.EscapeUriString(url);
                using (var webClient = new System.Net.WebClient())
                {
                    var imagenDescargada = webClient.DownloadData(urlEscapada);
                    using (var stream = new MemoryStream(imagenDescargada))
                    {
                        pbxArticulo.Image = Image.FromStream(stream);
                    }
                }
            }
            else
            {
                MessageBox.Show("No hay artículos o imágenes disponibles para mostrar.");
            }
            dgvListaArticulos.DataSource = listaArticulos;
            dgvListaArticulos.Columns["Id"].Visible = false;


        }
        //FILTRO RAPIDO
        private void txtFiltroRapido_TextChanged(object sender, EventArgs e)
        {
            List<Articulo> listaFiltrada;
            string filtro = txtFiltroRapido.Text;

            if (filtro.Length>0)
            {
                listaFiltrada = listaArticulos.FindAll(x => x.CodigoArticulo.ToUpper().Contains(filtro.ToUpper()) || x.Nombre.ToUpper().Contains(filtro.ToUpper()) || x.Descripcion.ToUpper().Contains(filtro.ToUpper()) || x.Marcas.NombreMarca.ToUpper().Contains(filtro.ToUpper()) || x.Categorias.NombreCategoria.ToUpper().Contains(filtro.ToUpper()));
                //listaFiltrada = listaArticulos.FindAll(x => x.CodigoArticulo.ToUpper().Contains(filtro.ToUpper()));

            }
            else
            {
                listaFiltrada = listaArticulos;
            }


            dgvListaArticulos.DataSource = null;
            dgvListaArticulos.DataSource = listaFiltrada;



            dgvListaArticulos.Columns["Id"].Visible = false;
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (dgvListaArticulos.SelectedRows.Count > 0)
            {

                modificando = true;
                Articulo seleccionado = (Articulo)dgvListaArticulos.CurrentRow.DataBoundItem;

                AgregarArticulo articulo = new AgregarArticulo(seleccionado, modificando);
                articulo.ShowDialog();
            }
            else
            {
                MessageBox.Show("No hay un artículo seleccionado");
            }
            
            

        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            ArticuloNegocio negocio = new ArticuloNegocio();
            Articulo seleccionado;
            try
            {
                if (dgvListaArticulos.SelectedRows.Count > 0)
                {
                    DialogResult respuesta = MessageBox.Show("Estas seguro que desea eliminarlo?", "Eliminando", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (respuesta == DialogResult.Yes)
                    {
                        seleccionado = (Articulo)dgvListaArticulos.CurrentRow.DataBoundItem;
                        negocio.eliminar(seleccionado.Id);
                        cargar();

                    }

                }
                else
                {
                    MessageBox.Show("No hay un artículo seleccionado");
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void txtFiltro_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnReiniciarLista_Click(object sender, EventArgs e)
        {
            cargar();
            cboCampo.Items.Add("Nombre");
            cboCampo.Items.Add("Marcas");
            cboCampo.Items.Add("Precio");
            cboCampo.Items.Add("Categorias");
            cboCampo.Items.Add("Descripcion");
            cboCampo.Items.Add("Codigo Articulo");
        }

        private void dgvListaArticulos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnVerDescripcion_Click(object sender, EventArgs e)
        {
            Articulo seleccionado = new Articulo();
            seleccionado = (Articulo)dgvListaArticulos.CurrentRow.DataBoundItem;
            Descripcion descripcion = new Descripcion(seleccionado);
            descripcion.ShowDialog();
        }
    }
}
