using System.Text.Json;
using System.Text.RegularExpressions;

namespace Comedor31;

public class FormTicket : Form
{
    // Listado de datos: menú del comedor
    private readonly List<Platillo> menu = new()
    {
        new Platillo { Nombre = "Pollo con arroz",    Categoria = "Plato fuerte", Precio = 45m },
        new Platillo { Nombre = "Milanesa con puré",  Categoria = "Plato fuerte", Precio = 50m },
        new Platillo { Nombre = "Sopa de verduras",   Categoria = "Entrada",      Precio = 20m },
        new Platillo { Nombre = "Frijoles charros",   Categoria = "Guarnición",   Precio = 15m },
        new Platillo { Nombre = "Agua de jamaica",    Categoria = "Bebida",       Precio = 12m },
        new Platillo { Nombre = "Gelatina",           Categoria = "Postre",       Precio = 10m }
    };

    private readonly List<LineaTicket> lineas = new();

    // Controles
    private readonly TextBox txtEmpleado = new();
    private readonly ComboBox cmbPlatillo = new();
    private readonly TextBox txtCantidad = new();
    private readonly Button btnAgregar = new();
    private readonly Button btnGuardar = new();
    private readonly Button btnLimpiar = new();
    private readonly ListBox lstTicket = new();
    private readonly Label lblTotal = new();

    public FormTicket()
    {
        Text = "Comedor industrial - Ticket de consumo";
        ClientSize = new Size(520, 470);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        ConstruirInterfaz();
    }

    private void ConstruirInterfaz()
    {
        var titulo = new Label
        {
            Text = "Ticket de consumo",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 90, 160),
            Location = new Point(20, 15),
            AutoSize = true
        };

        var lblEmpleado = new Label { Text = "Empleado:", Location = new Point(20, 65), AutoSize = true };
        txtEmpleado.Location = new Point(130, 62);
        txtEmpleado.Width = 250;

        var lblPlatillo = new Label { Text = "Platillo:", Location = new Point(20, 105), AutoSize = true };
        cmbPlatillo.Location = new Point(130, 102);
        cmbPlatillo.Width = 250;
        cmbPlatillo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPlatillo.DataSource = menu;
        cmbPlatillo.DisplayMember = "Nombre";

        var lblCantidad = new Label { Text = "Cantidad:", Location = new Point(20, 145), AutoSize = true };
        txtCantidad.Location = new Point(130, 142);
        txtCantidad.Width = 80;

        EstilizarBoton(btnAgregar, "Agregar", new Point(400, 100), Color.FromArgb(0, 120, 215));
        btnAgregar.Click += BtnAgregar_Click;

        lstTicket.Location = new Point(20, 190);
        lstTicket.Size = new Size(480, 180);

        lblTotal.Text = "Total: $0.00";
        lblTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        lblTotal.Location = new Point(20, 380);
        lblTotal.AutoSize = true;

        EstilizarBoton(btnGuardar, "Generar ticket (JSON)", new Point(20, 420), Color.SeaGreen);
        btnGuardar.Width = 200;
        btnGuardar.Click += BtnGuardar_Click;

        EstilizarBoton(btnLimpiar, "Limpiar", new Point(240, 420), Color.Gray);
        btnLimpiar.Click += BtnLimpiar_Click;

        Controls.AddRange(new Control[]
        {
            titulo, lblEmpleado, txtEmpleado, lblPlatillo, cmbPlatillo,
            lblCantidad, txtCantidad, btnAgregar, lstTicket, lblTotal,
            btnGuardar, btnLimpiar
        });
    }

    private static void EstilizarBoton(Button b, string texto, Point ubicacion, Color color)
    {
        b.Text = texto;
        b.Location = ubicacion;
        b.Size = new Size(100, 35);
        b.BackColor = color;
        b.ForeColor = Color.White;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.Cursor = Cursors.Hand;
    }

    // ---------- Validaciones ----------

    private bool ValidarEmpleado()
    {
        // Cadena: solo letras y espacios, entre 3 y 40 caracteres
        string nombre = txtEmpleado.Text.Trim();
        if (!Regex.IsMatch(nombre, @"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]{3,40}$"))
        {
            Aviso("Escribe el nombre del empleado (solo letras, de 3 a 40 caracteres).");
            txtEmpleado.Focus();
            return false;
        }
        return true;
    }

    private bool ValidarCantidad(out int cantidad)
    {
        // Entero: de 1 a 20
        if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad < 1 || cantidad > 20)
        {
            Aviso("La cantidad debe ser un número entero entre 1 y 20.");
            txtCantidad.Focus();
            return false;
        }
        return true;
    }

    private static void Aviso(string mensaje) =>
        MessageBox.Show(mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    // ---------- Eventos click ----------

    private void BtnAgregar_Click(object? sender, EventArgs e)
    {
        if (!ValidarEmpleado()) return;
        if (cmbPlatillo.SelectedItem is not Platillo p) { Aviso("Elige un platillo."); return; }
        if (!ValidarCantidad(out int cantidad)) return;

        lineas.Add(new LineaTicket { Platillo = p.Nombre, Cantidad = cantidad, Precio = p.Precio });
        txtCantidad.Clear();
        RefrescarTicket();
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (!ValidarEmpleado()) return;
        if (lineas.Count == 0) { Aviso("Agrega al menos un platillo al ticket."); return; }

        decimal total = lineas.Sum(l => l.Subtotal);
        var ticket = new
        {
            Empleado = txtEmpleado.Text.Trim(),
            Fecha = DateTime.Now,
            Consumo = lineas,
            Total = total
        };

        string carpeta = Path.Combine(AppContext.BaseDirectory, "tickets");
        Directory.CreateDirectory(carpeta);
        string archivo = Path.Combine(carpeta, $"ticket_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        string json = JsonSerializer.Serialize(ticket, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(archivo, json);

        MessageBox.Show($"Ticket guardado.\nTotal: {total:C}\n\n{archivo}", "Listo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        BtnLimpiar_Click(null, EventArgs.Empty);
    }

    private void BtnLimpiar_Click(object? sender, EventArgs e)
    {
        lineas.Clear();
        txtEmpleado.Clear();
        txtCantidad.Clear();
        RefrescarTicket();
    }

    // ---------- Cálculo del total ----------

    private void RefrescarTicket()
    {
        lstTicket.Items.Clear();
        foreach (var l in lineas)
            lstTicket.Items.Add($"{l.Cantidad} x {l.Platillo}  ({l.Precio:C})  =  {l.Subtotal:C}");

        lblTotal.Text = $"Total: {lineas.Sum(l => l.Subtotal):C}";
    }
}
