namespace BusinessLogicLayer.DTOs
{
  /*
  * DTO PARA CONSULTAS Y SALDOS
  */

    public class ResultDTO<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Datos { get; set; }
        public List<string> Errores { get; set; } = new List<string>();

        public static ResultDTO<T> Success(T datos, string mensaje = "Operación exitosa")
        {
            return new ResultDTO<T>
            {
                Exitoso = true,
                Mensaje = mensaje,
                Datos = datos
            };
        }

        public static ResultDTO<T> Failure(string mensaje, List<string>? errores = null)
        {
            return new ResultDTO<T>
            {
                Exitoso = false,
                Mensaje = mensaje,
                Errores = errores ?? new List<string>()
            };
        }
    }
}