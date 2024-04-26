using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductoController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductoController(IUnidadTrabajo unidadTrabajo, IWebHostEnvironment webHostEnvironment)
        {
            _unidadTrabajo = unidadTrabajo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        //Metodo Get Upert
        public async Task<IActionResult> Upsert(int? id)
        {
            ProductoVM productoVM = new ProductoVM()
            {
                Producto = new Producto(),
                CategoriaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownList("Categoria"),
                MarcaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownList("Marca"),
                PadreLista = _unidadTrabajo.Producto.ObtenerTodosDropdownList("Producto")
            };
            if(id ==  null)
            {
                //crear nuevo producto
                return View(productoVM);
            }
            else
            {
                //Actualizar al Producto
                productoVM.Producto = await _unidadTrabajo.Producto.obtener(id.GetValueOrDefault());
                if(productoVM.Producto == null)
                {
                    return NotFound();
                }
                return View(productoVM);
            }
        }






        #region API

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Upsert(ProductoVM productoVM)
        {
            if(ModelState.IsValid)
            {
                //definimos la variable para obtener los archivos desde el formulario
                //en este caso es una sola imagen
                var files = HttpContext.Request.Form.Files;

                //definos una variable para contruir la ruta del directorio WWWRoot
                string webRootPath = _webHostEnvironment.WebRootPath;

                if(productoVM.Producto.Id == 0)
                {
                    //Creación de un Producto Nuevo
                    //definimos la ruta completa de donde se guardara la img
                    string upload = webRootPath + DS.ImagenRuta;
                    
                    //Creamos un ID unico de la Imagen
                    string fileName = Guid.NewGuid().ToString();

                    //Crear la variable con el tipo de archiv de imagen (extensión)
                    string extension = Path.GetExtension(files[0].FileName);

                    //ahora habilitemos el manejos del streaming de los archivos
                    using(var fileStream = new FileStream(
                        Path.Combine(upload,fileName+extension)
                        ,FileMode.Create))
                    {
                        //copiamos el archivo de la memoria del navegador a la 
                        //carpeta del servidor
                        files[0].CopyTo(fileStream);
                    }
                    //asignamos el nombre de la imagen del producto hacia su registro
                    productoVM.Producto.ImagenUrl = fileName + extension;
                    //preparamos al modelo para ser guardado
                    await _unidadTrabajo.Producto.Agregar(productoVM.Producto);

                }
                else
                {
                    //Actualizar un Producto existente
                    //hacemos la consulta del registro a modificar
                    var objProducto = await _unidadTrabajo.Producto
                        .ObtenerPrimero(p => p.Id == productoVM.Producto.Id
                        ,isTracking:false);

                    //saber si el usuario eligio otra imagen
                    if(files.Count > 0)
                    {
                        string upload = webRootPath + DS.ImagenRuta;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        //borrar la imagen anterior
                        var anteriorFile = Path.Combine(upload, objProducto.ImagenUrl);

                        //Verificamos si la imagen existe
                        if (System.IO.File.Exists(anteriorFile))
                        {
                            System.IO.File.Delete(anteriorFile);
                        }
                        //creamos la nueva imagen
                        using (var fileStream = new FileStream(
                                Path.Combine(upload, fileName + extension)
                                ,FileMode.Create))
                                {
                                    files[0].CopyTo(fileStream);
                                }
                        //asignamos la nueva imagen al registro del producto
                        productoVM.Producto.ImagenUrl = fileName + extension;
                    }
                    //si no se eligio otra imagen
                    else
                    {
                        productoVM.Producto.ImagenUrl = objProducto.ImagenUrl;
                    }
                    _unidadTrabajo.Producto.Actualizar(productoVM.Producto);
                }
                TempData[DS.Exitosa] = "Producto Registrado con Exito";
                await _unidadTrabajo.Guardar();
                return View("Index");
            }
            //si el modelState es Invalido
            TempData[DS.Error] = "Algo anda MAl";
            productoVM.CategoriaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownList("Categoria");
            productoVM.MarcaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownList("Marca");
            productoVM.PadreLista = _unidadTrabajo.Producto.ObtenerTodosDropdownList("Producto");
            return View(productoVM);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Producto.ObtenerTodos(incluirPropiedades:"Categoria,Marca");

            return Json(new {data = todos});
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var productoDb = await _unidadTrabajo.Producto.obtener(id);
            if (productoDb == null)
            {
                return Json(new { success = false, message = "Error al Borrar el Producto" });
            }
            //Eliminamos la imagen
            string upload = _webHostEnvironment.WebRootPath + DS.ImagenRuta;
            var anteriorFile = Path.Combine(upload, productoDb.ImagenUrl);
            if (System.IO.File.Exists(anteriorFile))
            {
                System.IO.File.Delete(anteriorFile);
            }

            _unidadTrabajo.Producto.Remover(productoDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Producto Eliminado con Exito" });
        }


        [ActionName("ValidarSerie")]
        public async Task<IActionResult>ValidarSerie(string serie,int id = 0)
        {
            bool valor=false;
            var lista = await _unidadTrabajo.Producto.ObtenerTodos();
            if(id == 0)
            {
                valor = lista.Any(b=>b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim());
            }
            else
            {
                valor = lista.Any(b=>b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim() && b.Id !=id);
            }

            if (valor)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }



        #endregion
    }
}
