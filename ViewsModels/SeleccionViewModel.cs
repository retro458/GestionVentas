using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionVentas.ViewsModels
{
    public class SeleccionViewModel
    {
        public int EmpresaID { get; set; }
        public int ClienteID { get; set; }
        public List<SelectListItem> Empresas { get; set; } 
        public List<SelectListItem> Clientes { get; set; } 
    }
}
