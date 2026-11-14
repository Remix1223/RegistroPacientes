SqliteConnection con = new SqliteConnection("Data Source=pacientes.db;Foreign Keys=True");

try
{
    con.Open();

    SqliteCommand tabla = con.CreateCommand();
    tabla.CommandText = "CREATE TABLE IF NOT EXISTS Pacientes (Id INTEGER PRIMARY KEY AUTOINCREMENT, Cedula TEXT NOT NULL UNIQUE, Nombre TEXT NOT NULL, Apellido TEXT NOT NULL, FechaNacimiento TEXT NOT NULL, Telefono TEXT NOT NULL)";
    tabla.ExecuteNonQuery();
    tabla.CommandText = "CREATE TABLE IF NOT EXISTS Consultas (Id INTEGER PRIMARY KEY AUTOINCREMENT, PacienteId INTEGER NOT NULL, Fecha TEXT NOT NULL, Motivo TEXT NOT NULL, Diagnostico TEXT NOT NULL, FOREIGN KEY (PacienteId) REFERENCES Pacientes(Id))";
    tabla.ExecuteNonQuery();
}
catch (Exception ex)
{
    Console.WriteLine("No se pudo preparar la base de datos: " + ex.Message);
    return;
}

string selectPacientes = "SELECT Id, Cedula, Nombre, Apellido, FechaNacimiento, Telefono, CAST(strftime('%Y','now') AS INTEGER) - CAST(strftime('%Y', FechaNacimiento) AS INTEGER) - (strftime('%m-%d','now') < strftime('%m-%d', FechaNacimiento)) AS Edad FROM Pacientes ";
string opcion = "";

while (opcion != "8")
{
    Console.WriteLine();
    Console.WriteLine("REGISTRO DE PACIENTES");
    Console.WriteLine("1. Registrar paciente");
    Console.WriteLine("2. Buscar paciente");
    Console.WriteLine("3. Modificar paciente");
    Console.WriteLine("4. Eliminar paciente");
    Console.WriteLine("5. Listar pacientes");
    Console.WriteLine("6. Registrar consulta");
    Console.WriteLine("7. Ver consultas de un paciente");
    Console.WriteLine("8. Salir");
    Console.Write("Opcion: ");
    opcion = (Console.ReadLine() ?? "").Trim();

    string? filtro = null;
    object valor = "";

    try
    {
        switch (opcion)
        {
            case "1":
                {
                    Console.Write("Cedula: ");
                    string cedula = (Console.ReadLine() ?? "").Replace("-", "").Replace(" ", "").Trim();
                    Console.Write("Nombre: ");
                    string nombre = (Console.ReadLine() ?? "").Trim();
                    Console.Write("Apellido: ");
                    string apellido = (Console.ReadLine() ?? "").Trim();
                    Console.Write("Fecha de nacimiento (dd/mm/aaaa): ");
                    DateTime fecha;
                    while (!DateTime.TryParse(Console.ReadLine(), out fecha))
                    {
                        Console.Write("Fecha invalida, use el formato dd/mm/aaaa: ");
                    }
                    Console.Write("Telefono: ");
                    string telefono = (Console.ReadLine() ?? "").Replace("-", "").Replace(" ", "").Trim();

                    if (cedula.Length != 11 || !cedula.All(char.IsDigit))
                    {
                        throw new Exception("La cedula debe tener 11 digitos.");
                    }

                    if (nombre == "" || apellido == "")
                    {
                        throw new Exception("El nombre y el apellido no pueden estar vacios.");
                    }

                    if (fecha > DateTime.Today || fecha.Year < 1900)
                    {
                        throw new Exception("La fecha de nacimiento no es valida.");
                    }

                    if (telefono.Length != 10 || !telefono.All(char.IsDigit))
                    {
                        throw new Exception("El telefono debe tener 10 digitos.");
                    }

                    SqliteCommand repetida = con.CreateCommand();
                    repetida.CommandText = "SELECT COUNT(*) FROM Pacientes WHERE Cedula = @c";
                    repetida.Parameters.AddWithValue("@c", cedula);

                    if (Convert.ToInt32(repetida.ExecuteScalar()) > 0)
                    {
                        throw new Exception("Ya existe un paciente con esa cedula.");
                    }

                    SqliteCommand insertar = con.CreateCommand();
                    insertar.CommandText = "INSERT INTO Pacientes (Cedula, Nombre, Apellido, FechaNacimiento, Telefono) VALUES (@c, @n, @a, @f, @t)";
                    insertar.Parameters.AddWithValue("@c", cedula);
                    insertar.Parameters.AddWithValue("@n", nombre);
                    insertar.Parameters.AddWithValue("@a", apellido);
                    insertar.Parameters.AddWithValue("@f", fecha.ToString("yyyy-MM-dd"));
                    insertar.Parameters.AddWithValue("@t", telefono);
                    insertar.ExecuteNonQuery();

                    Console.WriteLine("Paciente registrado.");
                    break;
                }
            case "2":
                {
                    Console.WriteLine("1. Por ID");
                    Console.WriteLine("2. Por cedula");
                    Console.WriteLine("3. Por nombre o apellido");
                    Console.Write("Buscar por: ");
                    string criterio = (Console.ReadLine() ?? "").Trim();

                    if (criterio == "1")
                    {
                        Console.Write("ID: ");
                        int idBuscado;
                        while (!int.TryParse(Console.ReadLine(), out idBuscado))
                        {
                            Console.Write("Numero invalido, intente de nuevo: ");
                        }
                        filtro = "WHERE Id = @v";
                        valor = idBuscado;
                    }
                    else if (criterio == "2")
                    {
                        Console.Write("Cedula: ");
                        valor = (Console.ReadLine() ?? "").Replace("-", "").Replace(" ", "").Trim();
                        filtro = "WHERE Cedula = @v";
                    }
                    else if (criterio == "3")
                    {
                        Console.Write("Nombre o apellido: ");
                        valor = "%" + (Console.ReadLine() ?? "").Trim() + "%";
                        filtro = "WHERE Nombre LIKE @v OR Apellido LIKE @v";
                    }
                    else
                    {
                        Console.WriteLine("Opcion invalida.");
                    }
                    break;
                }
            case "3":
                {
                    Console.Write("ID del paciente a modificar: ");
                    int id;
                    while (!int.TryParse(Console.ReadLine(), out id))
                    {
                        Console.Write("Numero invalido, intente de nuevo: ");
                    }

                    SqliteCommand verificar = con.CreateCommand();
                    verificar.CommandText = "SELECT COUNT(*) FROM Pacientes WHERE Id = @id";
                    verificar.Parameters.AddWithValue("@id", id);

                    if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                    {
                        throw new Exception("No existe un paciente con ese ID.");
                    }

                    Console.Write("Nueva cedula: ");
                    string cedula = (Console.ReadLine() ?? "").Replace("-", "").Replace(" ", "").Trim();
                    Console.Write("Nuevo nombre: ");
                    string nombre = (Console.ReadLine() ?? "").Trim();
                    Console.Write("Nuevo apellido: ");
                    string apellido = (Console.ReadLine() ?? "").Trim();
                    Console.Write("Nueva fecha de nacimiento (dd/mm/aaaa): ");
                    DateTime fecha;
                    while (!DateTime.TryParse(Console.ReadLine(), out fecha))
                    {
                        Console.Write("Fecha invalida, use el formato dd/mm/aaaa: ");
                    }
                    Console.Write("Nuevo telefono: ");
                    string telefono = (Console.ReadLine() ?? "").Replace("-", "").Replace(" ", "").Trim();

                    if (cedula.Length != 11 || !cedula.All(char.IsDigit))
                    {
                        throw new Exception("La cedula debe tener 11 digitos.");
                    }

                    if (nombre == "" || apellido == "")
                    {
                        throw new Exception("El nombre y el apellido no pueden estar vacios.");
                    }

                    if (fecha > DateTime.Today || fecha.Year < 1900)
                    {
                        throw new Exception("La fecha de nacimiento no es valida.");
                    }

                    if (telefono.Length != 10 || !telefono.All(char.IsDigit))
                    {
                        throw new Exception("El telefono debe tener 10 digitos.");
                    }

                    SqliteCommand repetida = con.CreateCommand();
                    repetida.CommandText = "SELECT COUNT(*) FROM Pacientes WHERE Cedula = @c AND Id <> @id";
                    repetida.Parameters.AddWithValue("@c", cedula);
                    repetida.Parameters.AddWithValue("@id", id);

                    if (Convert.ToInt32(repetida.ExecuteScalar()) > 0)
                    {
                        throw new Exception("Ya existe otro paciente con esa cedula.");
                    }

                    SqliteCommand actualizar = con.CreateCommand();
                    actualizar.CommandText = "UPDATE Pacientes SET Cedula = @c, Nombre = @n, Apellido = @a, FechaNacimiento = @f, Telefono = @t WHERE Id = @id";
                    actualizar.Parameters.AddWithValue("@c", cedula);
                    actualizar.Parameters.AddWithValue("@n", nombre);
                    actualizar.Parameters.AddWithValue("@a", apellido);
                    actualizar.Parameters.AddWithValue("@f", fecha.ToString("yyyy-MM-dd"));
                    actualizar.Parameters.AddWithValue("@t", telefono);
                    actualizar.Parameters.AddWithValue("@id", id);
                    actualizar.ExecuteNonQuery();

                    Console.WriteLine("Paciente modificado.");
                    break;
                }
            case "4":
                {
                    Console.Write("ID del paciente a eliminar: ");
                    int id;
                    while (!int.TryParse(Console.ReadLine(), out id))
                    {
                        Console.Write("Numero invalido, intente de nuevo: ");
                    }

                    SqliteCommand verificar = con.CreateCommand();
                    verificar.CommandText = "SELECT COUNT(*) FROM Pacientes WHERE Id = @id";
                    verificar.Parameters.AddWithValue("@id", id);

                    if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                    {
                        throw new Exception("No existe un paciente con ese ID.");
                    }

                    Console.Write("Se eliminara el paciente y sus consultas. Escriba s para confirmar: ");
                    string respuesta = (Console.ReadLine() ?? "").Trim().ToLower();

                    if (respuesta == "s")
                    {
                        SqliteCommand borrar = con.CreateCommand();
                        borrar.CommandText = "DELETE FROM Consultas WHERE PacienteId = @id";
                        borrar.Parameters.AddWithValue("@id", id);
                        borrar.ExecuteNonQuery();
                        borrar.CommandText = "DELETE FROM Pacientes WHERE Id = @id";
                        borrar.ExecuteNonQuery();
                        Console.WriteLine("Paciente eliminado.");
                    }
                    else
                    {
                        Console.WriteLine("Operacion cancelada.");
                    }
                    break;
                }
            case "5":
                {
                    filtro = "";
                    break;
                }
            case "6":
                {
                    Console.Write("ID del paciente: ");
                    int id;
                    while (!int.TryParse(Console.ReadLine(), out id))
                    {
                        Console.Write("Numero invalido, intente de nuevo: ");
                    }

                    SqliteCommand verificar = con.CreateCommand();
                    verificar.CommandText = "SELECT COUNT(*) FROM Pacientes WHERE Id = @id";
                    verificar.Parameters.AddWithValue("@id", id);

                    if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                    {
                        throw new Exception("No existe un paciente con ese ID.");
                    }

                    Console.Write("Motivo de la consulta: ");
                    string motivo = (Console.ReadLine() ?? "").Trim();

                    if (motivo == "")
                    {
                        throw new Exception("El motivo de la consulta no puede estar vacio.");
                    }

                    Console.Write("Diagnostico: ");
                    string diagnostico = (Console.ReadLine() ?? "").Trim();

                    SqliteCommand nueva = con.CreateCommand();
                    nueva.CommandText = "INSERT INTO Consultas (PacienteId, Fecha, Motivo, Diagnostico) VALUES (@p, @f, @m, @d)";
                    nueva.Parameters.AddWithValue("@p", id);
                    nueva.Parameters.AddWithValue("@f", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    nueva.Parameters.AddWithValue("@m", motivo);
                    nueva.Parameters.AddWithValue("@d", diagnostico);
                    nueva.ExecuteNonQuery();

                    Console.WriteLine("Consulta registrada.");
                    break;
                }
            case "7":
                {
                    Console.Write("ID del paciente: ");
                    int id;
                    while (!int.TryParse(Console.ReadLine(), out id))
                    {
                        Console.Write("Numero invalido, intente de nuevo: ");
                    }

                    SqliteCommand verificar = con.CreateCommand();
                    verificar.CommandText = "SELECT COUNT(*) FROM Pacientes WHERE Id = @id";
                    verificar.Parameters.AddWithValue("@id", id);

                    if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                    {
                        throw new Exception("No existe un paciente con ese ID.");
                    }

                    List<Consulta> consultas = new List<Consulta>();
                    SqliteCommand leer = con.CreateCommand();
                    leer.CommandText = "SELECT Id, PacienteId, Fecha, Motivo, Diagnostico FROM Consultas WHERE PacienteId = @id ORDER BY Id";
                    leer.Parameters.AddWithValue("@id", id);
                    SqliteDataReader lectorConsultas = leer.ExecuteReader();

                    while (lectorConsultas.Read())
                    {
                        consultas.Add(new Consulta(
                            lectorConsultas.GetInt32(0),
                            lectorConsultas.GetInt32(1),
                            DateTime.Parse(lectorConsultas.GetString(2)),
                            lectorConsultas.GetString(3),
                            lectorConsultas.GetString(4)));
                    }

                    lectorConsultas.Close();

                    if (consultas.Count == 0)
                    {
                        Console.WriteLine("Este paciente no tiene consultas.");
                    }

                    foreach (Consulta c in consultas)
                    {
                        Console.WriteLine(c.Fecha.ToString("dd/MM/yyyy HH:mm") + " - " + c.Motivo + " - " + c.Diagnostico);
                    }
                    break;
                }
            case "8":
                {
                    Console.WriteLine("Fin del programa.");
                    break;
                }
            default:
                {
                    Console.WriteLine("Opcion invalida.");
                    break;
                }
        }

        if (filtro != null)
        {
            SqliteCommand buscar = con.CreateCommand();
            buscar.CommandText = selectPacientes + filtro + " ORDER BY Id";

            if (filtro != "")
            {
                buscar.Parameters.AddWithValue("@v", valor);
            }

            List<Paciente> pacientes = new List<Paciente>();
            SqliteDataReader lector = buscar.ExecuteReader();

            while (lector.Read())
            {
                pacientes.Add(new Paciente(
                    lector.GetInt32(0),
                    lector.GetString(1),
                    lector.GetString(2),
                    lector.GetString(3),
                    DateTime.Parse(lector.GetString(4)),
                    lector.GetString(5),
                    lector.GetInt32(6)));
            }

            lector.Close();

            if (pacientes.Count == 0)
            {
                Console.WriteLine("No se encontraron pacientes.");
            }

            foreach (Paciente p in pacientes)
            {
                Console.WriteLine(p.Id + " - " + p.Cedula + " - " + p.Nombre + " " + p.Apellido + " - " + p.Edad + " anios - " + p.Telefono);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }
}

con.Close();