using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Helpers
{
    public static class CONSTANTES
    {
        public static string DATETIME_DATABASE_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public static string DATETIME_HUMAN_FORMAT = "ddd dd de MMMM del yyyy, a las HH:mm:ss";

        public static int SUPER_ADMIN_ROL = 1;
        
        public static string ERROR_MESSAGE = "<strong>Hubo un error.</strong> Por favor, llene todos los campos.";
        public static string SUCCESS_MESSAGE = "<strong>Actualizado.</strong> Los datos se han guardado correctamente.";
        public static string SUCCESS = "success";
        public static string ERROR = "error";
        public static string ERROR_UPDATE_MESSAGE = "<strong>Hubo un error al actualizar.</strong> Por favor, verifique la información a actualizar.";
        public static string ERROR_INSERT_MESSAGE = "<strong>Hubo un error al insertar.</strong> Por favor, verifique que la información ingresada sea correcta.";
        public static string ERROR_RECOVERY_PASSWORD = "<strong>La cuenta o correo ingresado no existe.</strong> Por favor, verifique la información.";
        public static string ERROR_ROL_PERMISSION = "<strong>Hubo un error al realizar la acción.</strong> Por favor, verifique que su rol cuenta con los privilegios necesarios para realizar la operación.";
        public static string SUCCESS_RECOVERY_PASSWORD = "<strong>Se ha enviado un correo con la contraseña.</strong>";

        public static string STATUS_FIELD = "status";
        public static string MESSAGE_FIELD = "message";

        public static int NRO_COLUMNAS = 12;

        public static string VALIDATE_MESSAGE_PRIVACIDAD = "Por favor, lea y acepte las Políticas de Privacidad y Condiciones de Uso para poder enviar su consulta";

        public static string URL_BITLY_API = "https://api-ssl.bitly.com/v3/shorten";

        public static int ROL_ADMIN = 2;
        public static int ROL_RESPONSABLE = 3;

        public static string ERROR_SELECT_RESPONSABLE = "<strong>Hubo un error.</strong> Por favor, seleccione al menos un responsable.";
    }

}
