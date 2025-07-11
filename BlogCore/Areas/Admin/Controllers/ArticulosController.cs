using BlogCore.AccesoDatos.Data.Repository.IRepository;
using BlogCore.Models.Services;
using BlogCore.Models.ViewModels;
using BlogCore.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogCore.Areas.Admin.Controllers
{
    [Authorize(Roles = CNT.GestorDeTickets + "," + CNT.AgenteSoporte)]
    [Area("Admin")]
    public class ArticulosController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ArticulosController(IContenedorTrabajo contenedorTrabajo,
            IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            ArticuloVM artiVM = new ArticuloVM()
            {
                Articulo = new BlogCore.Models.Articulo(),
                ListaCategorias = _contenedorTrabajo.Categoria.GetListaCategorias()
            };


            return View(artiVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ArticuloVM artiVM)
        {
            if (ModelState.IsValid)
            {
                string rutaPrincipal = _hostingEnvironment.WebRootPath;
                var archivos = HttpContext.Request.Form.Files;
                if (artiVM.Articulo.Id == 0 && archivos.Count() > 0)
                {
                    //Nuevo articulo
                    string nombreArchivo = Guid.NewGuid().ToString();
                    var subidas = Path.Combine(rutaPrincipal, @"imagenes\articulos");
                    var extension = Path.GetExtension(archivos[0].FileName);

                    using (var fileStreams = new FileStream(Path.Combine(subidas, nombreArchivo + extension), FileMode.Create))
                    {
                        archivos[0].CopyTo(fileStreams);
                    }

                    artiVM.Articulo.UrlImagen = @"\imagenes\articulos\" + nombreArchivo + extension;
                    artiVM.Articulo.FechaCreacion = DateTime.Now.ToString();

                    _contenedorTrabajo.Articulo.Add(artiVM.Articulo);
                    _contenedorTrabajo.Save();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Imagen", "Debes seleccionar una imagen");
                }

            }

            artiVM.ListaCategorias = _contenedorTrabajo.Categoria.GetListaCategorias();
            return View(artiVM);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            ArticuloVM artiVM = new ArticuloVM()
            {
                Articulo = new BlogCore.Models.Articulo(),
                ListaCategorias = _contenedorTrabajo.Categoria.GetListaCategorias()
            };

            if (id != null)
            {
                artiVM.Articulo = _contenedorTrabajo.Articulo.Get(id.GetValueOrDefault());
            }

            return View(artiVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ArticuloVM artiVM)
        {
            if (ModelState.IsValid)
            {
                string rutaPrincipal = _hostingEnvironment.WebRootPath;
                var archivos = HttpContext.Request.Form.Files;

                var articuloDesdeBd = _contenedorTrabajo.Articulo.Get(artiVM.Articulo.Id);


                if (archivos.Count() > 0)
                {
                    //Nuevo imagen para el artículo
                    string nombreArchivo = Guid.NewGuid().ToString();
                    var subidas = Path.Combine(rutaPrincipal, @"imagenes\articulos");
                    var extension = Path.GetExtension(archivos[0].FileName);
                    var nuevaExtension = Path.GetExtension(archivos[0].FileName);

                    var rutaImagen = Path.Combine(rutaPrincipal, articuloDesdeBd.UrlImagen.TrimStart('\\'));

                    if (System.IO.File.Exists(rutaImagen))
                    {
                        System.IO.File.Delete(rutaImagen);
                    }

                    //Nuevamente subimos el archivo
                    using (var fileStreams = new FileStream(Path.Combine(subidas, nombreArchivo + extension), FileMode.Create))
                    {
                        archivos[0].CopyTo(fileStreams);
                    }

                    artiVM.Articulo.UrlImagen = @"\imagenes\articulos\" + nombreArchivo + extension;
                    artiVM.Articulo.FechaCreacion = DateTime.Now.ToString();

                    _contenedorTrabajo.Articulo.Update(artiVM.Articulo);
                    _contenedorTrabajo.Save();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    //Aquí sería cuando la imagen ya existe y se conserva
                    artiVM.Articulo.UrlImagen = articuloDesdeBd.UrlImagen;
                }

                _contenedorTrabajo.Articulo.Update(artiVM.Articulo);
                _contenedorTrabajo.Save();

                return RedirectToAction(nameof(Index));

            }

            artiVM.ListaCategorias = _contenedorTrabajo.Categoria.GetListaCategorias();
            return View(artiVM);
        }
        #region LLamadas a la API
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Articulo.GetAll(includeProperties: "Categoria") });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var articuloDesdeBd = _contenedorTrabajo.Articulo.Get(id);
            string rutaDirectorioPrincipal = _hostingEnvironment.WebRootPath;
            var rutaImagen = Path.Combine(rutaDirectorioPrincipal, articuloDesdeBd.UrlImagen.TrimStart('\\'));
            if (System.IO.File.Exists(rutaImagen))
            {
                System.IO.File.Delete(rutaImagen);
            }


            if (articuloDesdeBd == null)
            {
                return Json(new { success = false, message = "Error borrando artículo" });
            }

            _contenedorTrabajo.Articulo.Remove(articuloDesdeBd);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Artículo Borrado Correctamente" });
        }
        #endregion

        //Metodo para probar el kms

        public IActionResult ProbarKms()
        {
             
            var rutaCredenciales = "C:\\Users\\luisp\\Downloads\\dark-star-465316-e8-5ae99cd6df1b.json";

            var kms = new KmsService(rutaCredenciales);
            var texto = "Artículo secreto de BlogCore";

            var cifrado = kms.Cifrar(texto);
            var descifrado = kms.Descifrar(cifrado);

            return Content($"Original: {texto}\n\nCifrado: {cifrado}\n\nDescifrado: {descifrado}");
        }

        //Metodo para exponer los articulos como admin

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "GestorDeTickets")]
        public IActionResult ObtenerArticuloCifrado(int id)
        {
            var articulo = _contenedorTrabajo.Articulo.Get(id);
            if (articulo == null)
                return NotFound();

            var kms = new KmsService("C:\\Users\\luisp\\Downloads\\dark-star-465316-e8-5ae99cd6df1b.json");
            var contenidoCifrado = kms.Cifrar(articulo.Descripcion);


            var dto = new ArticuloCifradoDTO
            {
                Id = articulo.Id,
                Nombre = articulo.Nombre,
                ContenidoCifrado = contenidoCifrado
            };

            return Ok(dto);
        }
    }

}
