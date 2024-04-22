﻿using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Utilidades;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductoController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        public ProductoController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            return View();
        }

        

        


        #region API
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
            _unidadTrabajo.Producto.Remover(productoDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Producto Eliminada con Exito" });
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
