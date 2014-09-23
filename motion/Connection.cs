using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace motion
{
    class Connection
    {

        public MySqlConnection getConnection(string data)
        {
            MySqlConnection myConn = new MySqlConnection(this.data);
            return myConn;

        }//--------------------------------------------------------------------------

        public List<String> getCategories()
        {
            MySqlConnection myConn = getConnection(data);
            MySqlCommand command = myConn.CreateCommand();
            command.CommandText = "SELECT c.sNombre FROM aretemotion.tblOrganizaciones as o join  aretemotion.tblCategorias as c on o.id = c.idOrganizacion where o.sNombre='" + company + "';";
            try
            {
                myConn.Open();
                command.ExecuteNonQuery();
                MySqlDataReader reader = command.ExecuteReader();
                List<String> categorys = new List<String>();
                while (reader.Read())
                {
                    categorys.Add(reader["sNombre"] + "");

                }

                //cerrar la conexion
                myConn.Close();
                if (categorys.Count > 0)
                {
                    Console.WriteLine("_____________________ Existen CATEGORIAS");
                    return categorys;
                }
                else
                {
                    Console.WriteLine("_____________________ No hay CATEGORIAS");
                    return null;
                }


            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                Console.WriteLine("_____________________ FALLO EN LA CONEXIÓN A LA BD");
                return null;
            }


        }//-------------------------------------------------------------------
        public string[] getDataCompany()
        {
            string[] dataCompany = new string[3];
            string c = this.company.ToLower();
            MySqlConnection myConn = getConnection(data);
            MySqlCommand command = myConn.CreateCommand();
            command.CommandText = "SELECT * FROM aretemotion.tblOrganizaciones WHERE sNombre ='" + c + "';";
            try
            {
                myConn.Open();
                command.ExecuteNonQuery();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dataCompany[0] = reader["ID"] + "";
                    dataCompany[1] = reader["Snombre"] + "";
                    dataCompany[2] = reader["sConfiguracion"] + "";

                    Console.WriteLine("SE GUARDARON LOS DATOS DE LA COMPAÑIA");
                }
                myConn.Close();
                //guardar los datos de la compañia
                return dataCompany;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

        }//---------------------------------------------------------



        public List<Set> getLinksCompany(string category)
        {

            MySqlConnection myConn = getConnection(data);
            MySqlCommand command = myConn.CreateCommand();
            //command.CommandText = "SELECT o.sNombre,o.sConfiguracion,c.sNombre,c.ID,p.idSet,p.sUrl from aretemotion.tblOrganizaciones as o join aretemotion.tblCategorias as c on o.id = c.idOrganizacion join aretemotion.tblSets as s on c.id = s.idCategoria join aretemotion.tblPantallas as p on s.id = p.idSet where o.sNombre = '" + company + "' and p.idSet = " + numberSet + " and c.sNombre= '" + category + "';";

            command.CommandText = "SELECT o.sNombre,o.sConfiguracion,c.sNombre,c.ID,p.idSet,p.sUrl , s.iOrden, p.iOrdenPantalla from aretemotion.tblOrganizaciones as o join aretemotion.tblCategorias as c on o.id = c.idOrganizacion join aretemotion.tblSets as s on c.id = s.idCategoria join aretemotion.tblPantallas as p on s.id = p.idSet  where o.sNombre = '" + company + "'  and c.sNombre= '" + category + "' order by iOrden ASC;";

            try
            {
                myConn.Open();
                command.ExecuteNonQuery();

                MySqlDataReader reader = command.ExecuteReader();

                createSets(reader);

                //cerrar la conexion
                myConn.Close();

                //regresa la lista de sets con sus diferentes links
                return sets;

            }
            catch (MySqlException ex)
            {
                Console.Write("error de ejecucion de consulta," + ex);
                return null;
            }


        }//---------------------------------------------------------

        public void createSets(MySqlDataReader reader)
        {
            int controlOffSet = 0;
            int count = 1;
            int numberOfSet = 1;

            if (sets.Count > 0)
            {
                sets.Clear();
            }

            while (reader.Read())
            {

                if (count == 1)
                {
                    Set s = new Set();
                    s.setNumberOfSet(numberOfSet);
                    controlOffSet = Int32.Parse(reader["idSet"] + "");  //el ID del link
                    Link link = new Link();
                    link.setOrderScreen(Int32.Parse(reader["iOrdenPantalla"] + ""));
                    link.setLink(reader["sURL"] + "");

                    s.setLink(link);

                    //agregar el set con el link incluido
                    sets.Add(s);

                }
                else
                { //para los demas elementos
                    //si sigue siendo el mismo id ya no se crea otro set solo se añade al ya creado

                    if (controlOffSet == Int32.Parse(reader["idSet"] + ""))
                    {
                        //si es el mismo idSet busca el set previamente agregado , al ultimo elemento le agega 
                        //manda el link, el numero de set , el orden de pantalla
                        insertLinkInSet(reader["sURL"] + "", Int32.Parse(reader["iOrdenPantalla"] + ""), numberOfSet);//inserta el link en el set al que pertenece

                        // Console.WriteLine("insertar en uno existente");
                        controlOffSet = Int32.Parse(reader["idSet"] + "");
                    }
                    else
                    { //si ya es otro id de set lo anade a aun nuevo objeto tipo set

                        // Console.WriteLine("es diferente por eso agrega un nuevo set");
                        numberOfSet += 1;

                        Set s = new Set();
                        s.setNumberOfSet(numberOfSet);
                        controlOffSet = Int32.Parse(reader["idSet"] + "");  //el ID del link
                        Link link = new Link();
                        link.setOrderScreen(Int32.Parse(reader["iOrdenPantalla"] + ""));
                        link.setLink(reader["sURL"] + "");

                        s.setLink(link);

                        //agregar el set con el link incluido
                        sets.Add(s);

                        //Console.WriteLine("crea nuevo set");
                    }

                }

                count += 1;

            }

        }//---------------------------------------------------------

        public void insertLinkInSet(string link, int ordenPantalla, int numberSet)
        {
            int last = sets.Count - 1;
            //al ultimo set agregado le agrega el link nuevo
            Set s = sets[last];
            Link l = new Link();
            l.setLink(link);
            s.setNumberOfSet(numberSet);
            s.setLink(l);


        }//---------------------------------------------------------


        //variables
        private string data = "datasource=173.194.243.107;port=3306;username=areteMotion;password=aretE";
        private string company = "arete";
        private List<Set> sets = new List<Set>();

    }
}
