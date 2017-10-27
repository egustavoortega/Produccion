using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Traking_Forms
{
    public class ClassData
    {

        public static string VlrCaptura = "";
        public static List<String> DatosEncontradosP = new List<string>();
        
        
        public static bool Marca_fecha = false;
        public static string estado = "";
        public static List<String> L_Marca_fecha = new List<string>();

        public static bool Nvlestudio = false;
        public static bool Braile = false;
        public static bool Invidente = false;
        public static bool Genero = false;
        public static bool Capacidad = false;

        public static SqlConnection conexion;

        public static string VlrAprobado = "";
        public static int  NumeroIntentosCaptura2 = 0;
        public static string UsuarioNumeroIntentosCaptura2 = "";
        public static bool Validacion = false;

        public static int Zoom = 0;
    }
}
