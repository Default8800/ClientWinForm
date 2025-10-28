using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ClientWinForm.DataClasses;
using Microsoft.VisualBasic.ApplicationServices;
namespace ClientWinForm
{
    public partial class Form1 : Form
    {

        private static int _idInterfaces = -1;
        private static int _idDevices = -1;
        private static int _idRegisters = -1;
        private List<object> _listRegisterValues = new List<object>();
        List<Registers> _backListRegisters = new List<Registers>();
        List<RegisterValues> _registerValuesList = new List<RegisterValues>();
        private bool _isSending = false; // Флаг блокировки таймера 
        private int _numTick = 0;
        private List<Devices> _devicesList = new List<Devices>();
        public Form1()
        {
            InitializeComponent();
            panel7.Paint += panel7_Paint;
        }
        private void DrawDevicesOnPanel()
        {
            panel7.Invalidate(); // Вызывает событие Paint
        }
        

        private void DrawDevice(Graphics g, Devices device)
        {
            if (!device.IsEnabled) return; // Не рисуем отключенные устройства

            // Преобразуем цвет из HEX в Color
            Color deviceColor = ColorTranslator.FromHtml(device.Color);

            using (SolidBrush brush = new SolidBrush(deviceColor))
            using (Pen pen = new Pen(Color.Black, 2))
            {
                switch (device.FigureType)
                {
                    case "Круг ●":
                        // Рисуем круг
                        g.FillEllipse(brush, device.PosX, device.PosY, device.Size, device.Size);
                        g.DrawEllipse(pen, device.PosX, device.PosY, device.Size, device.Size);
                        break;

                    
                    case "Квадрат ■":
                        // Рисуем квадрат
                        g.FillRectangle(brush, device.PosX, device.PosY, device.Size, device.Size);
                        g.DrawRectangle(pen, device.PosX, device.PosY, device.Size, device.Size);
                        break;

                    case "Треугольник ▲":
                        // Рисуем треугольник (равносторонний)
                        Point[] trianglePoints = new Point[]
                        {
                    new Point(device.PosX + device.Size / 2, device.PosY), // Верхняя точка
                    new Point(device.PosX, device.PosY + device.Size),     // Левая нижняя точка
                    new Point(device.PosX + device.Size, device.PosY + device.Size) // Правая нижняя точка
                        };
                        g.FillPolygon(brush, trianglePoints);
                        g.DrawPolygon(pen, trianglePoints);
                        break;
                }
            }

            // Добавляем текст с названием устройства
            using (Font font = new Font("Arial", 8))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(device.Name, font, textBrush, device.PosX, device.PosY - 15);
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void button3_Click(object sender, EventArgs e) // кнопка обновить интерфйейс
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);

            Interfaces _interface = new Interfaces(_idInterfaces, textBox1.Text, textBox2.Text);
            List<string> _interfaces = new List<string>();
            _interfaces.Add(_idInterfaces.ToString());
            _interfaces.Add(_interface.Name.ToString());
            _interfaces.Add(_interface.Description.ToString());
            _interfaces.Add(_interface.EditingDate.ToString());
            RequestsClass request = new RequestsClass("UpdateData", "Interfaces", _interfaces);

            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, options);
            Console.WriteLine($"📤 Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);

            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');

            Console.WriteLine($" Полученный JSON: {response_string}");
            Console.WriteLine($"Длина JSON: {response_string.Length}");

            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            // Десериализуем
            try
            {
                var response = JsonSerializer.Deserialize<ResponseGetAllData>(response_string);

                // Очищаем таблицу
                dataGridView1.Rows.Clear();

                // Добавляем строки через цикл
                foreach (var interfaceObj in response.ObjectsListInterfaces)
                {
                    dataGridView1.Rows.Add(
                        interfaceObj.Id,
                        interfaceObj.Name,
                        interfaceObj.Description,
                        interfaceObj.EditingDate
                    );
                }
                textBox1.Text = String.Empty;
                textBox2.Text = String.Empty;

            }
            catch (JsonException ex)
            {

                // Дополнительная диагностика
                foreach (char c in response_string)
                {
                    Console.WriteLine($"Символ: '{c}' Код: {(int)c:X}");
                }
            }
        }
        private async void LoadAllData(string typeObject)
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);

            Interfaces _interface = new Interfaces(textBox1.Text, textBox2.Text);
            List<string> _interfaces = new List<string>();
            _interfaces.Add("A");
            _interfaces.Add("A");
            _interfaces.Add("A");
            _interfaces.Add("A");
            RequestsClass request = new RequestsClass("GetAllData", "AllData", _interfaces);

            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, options);
            Console.WriteLine($"📤 Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);

            // Читаем ответ
            var buffer = new byte[8192];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');

            Console.WriteLine($" Полученный JSON: {response_string}");
            Console.WriteLine($"Длина JSON: {response_string.Length}");

            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            // Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, typeObject);
            }
            catch (JsonException ex)
            {

                // Дополнительная диагностика
                foreach (char c in response_string)
                {
                    Console.WriteLine($"Символ: '{c}' Код: {(int)c:X}");
                }
            }
        }
        private async void Form1_Load(object sender, EventArgs e)//загрузка формы
        {
            LoadAllData("Interfaces");
        }

        private async void button2_Click(object sender, EventArgs e)//копка удалить интерфейс
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);

            Interfaces _interface = new Interfaces(_idInterfaces, textBox1.Text, textBox2.Text);
            List<string> _interfaces = new List<string>();
            _interfaces.Add(_interface.Id.ToString());
            _interfaces.Add(_interface.Name.ToString());
            _interfaces.Add(_interface.Description.ToString());
            _interfaces.Add(_interface.EditingDate.ToString());
            RequestsClass request = new RequestsClass("DeleteData", "Interfaces", _interfaces);

            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);




            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, "Interfaces");
            }
            catch (JsonException ex)
            {

            }
            textBox1.Text = String.Empty;
            textBox2.Text = String.Empty;

        }

        private async void button1_Click(object sender, EventArgs e) // кнопка добавить интерфейс
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);

            Interfaces _interface = new Interfaces(textBox1.Text, textBox2.Text);
            List<string> _interfaces = new List<string>();
            _interfaces.Add(_interface.Id.ToString());
            _interfaces.Add(_interface.Name.ToString());
            _interfaces.Add(_interface.Description.ToString());
            _interfaces.Add(_interface.EditingDate.ToString());
            RequestsClass request = new RequestsClass("AddData", "Interfaces", _interfaces);

            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);




            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, "Interfaces");
            }
            catch (JsonException ex)
            {

            }
            textBox1.Text = String.Empty;
            textBox2.Text = String.Empty;
        }

        #region Обработка нажатия на гриды
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e) //грид интерфейсов
        {
            DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
            int i = (int)selectedRow.Cells[0].Value;
            _idInterfaces = i;
            GetOneData("Interfaces", i);
            //очистка параметров девайсов
            textBox3.Text = String.Empty;
            textBox4.Text = String.Empty;
            checkBox1.Checked = false;
            comboBox2.Text = "Выберите";
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
            comboBox3.Text = "Выберите";
            //очистка параметров регистров
            textBox5.Text = String.Empty;
            textBox6.Text = String.Empty;
            richTextBox1.Clear();
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;



            timer1.Stop();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            richTextBox1.Text = String.Empty;
            label13.Text = "История регистра:";
            label16.Text = "Актуальное значение:";
            button11.Enabled = false;
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            label12.Text = "Таймер:";
        }





        private async void GetOneData(string type, int id)
        {

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);
            RequestsClass request = new RequestsClass("", "", new List<string>());

            switch (type)
            {
                case "Interfaces":

                    List<string> _interfaces = new List<string>();
                    _interfaces.Add(id.ToString());
                    request = new RequestsClass("GetOneDataForUpdate", type, _interfaces);
                    break;
                case "Devices":
                    List<string> _devices = new List<string>();
                    _devices.Add(id.ToString());
                    request = new RequestsClass("GetOneDataForUpdate", type, _devices);
                    break;
                case "Registers":
                    List<string> _registers = new List<string>();
                    _registers.Add(_idRegisters.ToString());
                    request = new RequestsClass("GetOneDataForUpdate", type, _registers);
                    break;

            }



            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);

            // Читаем ответ
            var buffer = new byte[8000];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                var response = JsonSerializer.Deserialize<ResponseGetOneItem>(response_string);
                switch (response.TypeObjects)
                {
                    case "Interfaces":
                        if (response.Item is JsonElement jsonElement)
                        {
                            // Правильный вызов метода экземпляра
                            string jsonString = jsonElement.GetRawText();
                            Interfaces newInterface = JsonSerializer.Deserialize<Interfaces>(jsonString);

                            textBox1.Text = newInterface.Name;
                            textBox2.Text = newInterface.Description;
                            _idInterfaces = newInterface.Id;

                            LoadAllData("Devices");
                        }
                        break;
                    case "Devices":

                        if (response.Item is JsonElement jsonElement1)
                        {
                            // Правильный вызов метода экземпляра
                            string jsonString = jsonElement1.GetRawText();
                            Devices newDevice = JsonSerializer.Deserialize<Devices>(jsonString);


                            textBox3.Text = newDevice.Name;
                            textBox4.Text = newDevice.Description;
                            checkBox1.Checked = newDevice.IsEnabled;
                            comboBox2.Text = newDevice.FigureType;
                            numericUpDown1.Value = newDevice.Size;
                            numericUpDown2.Value = newDevice.PosX;
                            numericUpDown3.Value = newDevice.PosY;

                            switch (newDevice.Color)
                            {
                                case "#FF0000":
                                    comboBox3.SelectedIndex = 1;
                                    break;
                                case "#0000FF":
                                    comboBox3.SelectedIndex = 2;
                                    break;
                                case "#FFFF00":
                                    comboBox3.SelectedIndex = 3;
                                    break;
                                case "#008000":
                                    comboBox3.SelectedIndex = 4;
                                    break;

                            }
                            LoadAllData("Registers");

                        }
                        break;
                    case "Registers":
                        if (response.Item is JsonElement jsonElement2 && response.ObjectListRegistrsValue is List<object> registerValuesList)
                        {
                            string jsonString = jsonElement2.GetRawText();
                            Registers newRegisters = JsonSerializer.Deserialize<Registers>(jsonString);

                            // Сортируем список ПЕРЕД присваиванием
                            _listRegisterValues = registerValuesList
                                .Where(item => item is JsonElement)
                                .Select(item => (JsonElement)item)
                                .Select(element => new
                                {
                                    Element = element,
                                    RegisterValue = JsonSerializer.Deserialize<RegisterValues>(element.GetRawText())
                                })
                                .Where(x => x.RegisterValue.RegisterId == _idRegisters) // фильтруем по нужному регистру
                                .OrderBy(x => x.RegisterValue.Timestamp) // старые first, новые last
                                .Select(x => x.Element)
                                .Cast<object>()
                                .ToList();

                            textBox5.Text = newRegisters.Name;
                            textBox6.Text = newRegisters.Description;
                            label13.Text = "История регистра: " + newRegisters.Id;

                            // Очищаем RichTextBox
                            richTextBox1.Clear();
                            string lastValue = "";

                            // Используем ОТСОРТИРОВАННЫЙ _listRegisterValues вместо registerValuesList
                            for (int i = 0; i < _listRegisterValues.Count; i++)
                            {
                                if (_listRegisterValues[i] is JsonElement valueElement)
                                {
                                    try
                                    {
                                        var registerValue = JsonSerializer.Deserialize<RegisterValues>(valueElement.GetRawText());

                                        // Добавляем в RichTextBox
                                        richTextBox1.AppendText($"({registerValue.Timestamp:dd.MM.yyyy HH:mm:ss})  Значение: {registerValue.Value}\n");
                                        lastValue = registerValue.Value.ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        richTextBox1.AppendText($"Ошибка чтения значения {i + 1}: {ex.Message}\n");
                                    }
                                }
                            }
                            if (_listRegisterValues.Count > 0)
                            {
                                label16.Text = $"Актуальное значение: " + lastValue;
                                dateTimePicker1.Enabled = true;
                                dateTimePicker2.Enabled = true;

                                dateTimePicker1.Enabled = true;
                                dateTimePicker2.Enabled = true;
                                button11.Enabled = true;
                            }

                        }
                        break;
                }



            }
            catch (JsonException ex)
            {

            }
        }
        #endregion




        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        #region тут все что связано с девайсами
        private async void button4_Click(object sender, EventArgs e)//удалить
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);

            List<string> _devices = new List<string>();
            _devices.Add(_idDevices.ToString());

            RequestsClass request = new RequestsClass("DeleteData", "Devices", _devices);

            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);




            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, "Devices");
            }
            catch (JsonException ex)
            {

            }

            textBox3.Text = String.Empty;
            textBox4.Text = String.Empty;
            checkBox1.Checked = false;
            comboBox2.Text = "Выберите";
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
            comboBox3.Text = "Выберите";

        }

        private async void button5_Click(object sender, EventArgs e)//добавить
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);


            string _name = textBox3.Text;
            string _description = textBox4.Text;
            bool _enabled = checkBox1.Checked;
            string _figureType = comboBox2.Text;
            int _size = (int)numericUpDown1.Value;
            int _posX = (int)numericUpDown2.Value;
            int _posY = (int)numericUpDown3.Value;
            string _color = String.Empty;

            switch (comboBox3.SelectedIndex)
            {
                case 1:
                    _color = "#FF0000";
                    break;
                case 2:
                    _color = "#0000FF";
                    break;
                case 3:
                    _color = "#FFFF00";
                    break;
                case 4:
                    _color = "#008000";
                    break;
            }

            Devices _device = new Devices(_idInterfaces, _name, _description, _enabled, _figureType, _size, _posX, _posY, _color);
            List<string> _devices = new List<string>();
            _devices.Add(_device.Id.ToString());
            _devices.Add(_idInterfaces.ToString());
            _devices.Add(_name);
            _devices.Add(_description);
            _devices.Add(_enabled.ToString());
            _devices.Add(_figureType);
            _devices.Add(_size.ToString());
            _devices.Add(_posX.ToString());
            _devices.Add(_posY.ToString());
            _devices.Add(_color);
            RequestsClass request = new RequestsClass("AddData", "Devices", _devices);

            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);




            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, "Devices");
            }
            catch (JsonException ex)
            {

            }

            textBox3.Text = String.Empty;
            textBox4.Text = String.Empty;
            checkBox1.Checked = false;
            comboBox2.Text = "Выберите";
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
            comboBox3.Text = "Выберите";
        }


        private async Task AddDataToDataGridView(string responseFromData, string typeObject)
        {
            var response = JsonSerializer.Deserialize<ResponseGetAllData>(responseFromData);



            switch (typeObject)
            {
                case "Interfaces":
                    dataGridView1.Rows.Clear();

                    foreach (var interfaceObj in response.ObjectsListInterfaces)
                    {

                        dataGridView1.Rows.Add(
                            interfaceObj.Id,
                            interfaceObj.Name,
                            interfaceObj.Description,
                            interfaceObj.EditingDate
                        );
                    }
                    break;
                case "Devices":


                    dataGridView2.Rows.Clear();
                    _devicesList.Clear(); // Очищаем список устройств для отрисовки

                    foreach (var devicesObj in response.ObjectsListDevices)
                    {
                        if (devicesObj.InterfaceId == _idInterfaces)
                        {
                            dataGridView2.Rows.Add(
                            devicesObj.Id,
                            devicesObj.InterfaceId,
                            devicesObj.Name,
                            devicesObj.Description,
                            devicesObj.IsEnabled,
                            devicesObj.EditingDate,
                            devicesObj.FigureType,
                            devicesObj.Size,
                            devicesObj.PosX,
                            devicesObj.PosY,
                            devicesObj.Color
                            );

                            // Добавляем устройство в список для отрисовки
                            _devicesList.Add(devicesObj);
                        }
                    }

                    // Отрисовываем устройства на Panel
                    DrawDevicesOnPanel();

                    break;
                case "Registers":

                    dataGridView3.Rows.Clear();


                    foreach (var registersObj in response.ObjectsListRegisters)
                    {
                        _backListRegisters.Add(registersObj);
                        if (registersObj.DeviceId == _idDevices)
                        {
                            dataGridView3.Rows.Add(
                            registersObj.Id,
                            registersObj.DeviceId,
                            registersObj.Name,
                            registersObj.Description,
                            registersObj.EditingDate
                            );
                        }
                        button7.Enabled = true;
                        button8.Enabled = false;


                    }


                    break;
            }












        }

        private async void button6_Click(object sender, EventArgs e)//изменить
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);


            string _name = textBox3.Text;
            string _description = textBox4.Text;
            bool _enabled = checkBox1.Checked;
            string _figureType = comboBox2.Text;
            int _size = (int)numericUpDown1.Value;
            int _posX = (int)numericUpDown2.Value;
            int _posY = (int)numericUpDown3.Value;
            string _color = String.Empty;

            switch (comboBox3.SelectedIndex)
            {
                case 1:
                    _color = "#FF0000";
                    break;
                case 2:
                    _color = "#0000FF";
                    break;
                case 3:
                    _color = "#FFFF00";
                    break;
                case 4:
                    _color = "#008000";
                    break;
            }

            List<string> _devices = new List<string>();
            _devices.Add(_idDevices.ToString());
            _devices.Add(_idInterfaces.ToString());
            _devices.Add(_name);
            _devices.Add(_description);
            _devices.Add(_enabled.ToString());
            _devices.Add(_figureType);
            _devices.Add(_size.ToString());
            _devices.Add(_posX.ToString());
            _devices.Add(_posY.ToString());
            _devices.Add(_color);
            RequestsClass request = new RequestsClass("UpdateData", "Devices", _devices);

            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);




            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, "Devices");
            }
            catch (JsonException ex)
            {

            }

            textBox3.Text = String.Empty;
            textBox4.Text = String.Empty;
            checkBox1.Checked = false;
            comboBox2.Text = "Выберите";
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
            comboBox3.Text = "Выберите";
        }
        #endregion

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            DataGridViewRow selectedRow = dataGridView2.Rows[e.RowIndex];
            int i = (int)selectedRow.Cells[0].Value;
            _idDevices = i;
            GetOneData("Devices", i);


        }

        private void button7_Click(object sender, EventArgs e)
        {
            _numTick = 0;
            timer1.Start();
            timer1.Enabled = true;
            timer2.Start();
            button7.Enabled = false;
            button8.Enabled = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _numTick = 0;
            timer1.Enabled = false;
            button7.Enabled = true;
            button8.Enabled = false;
        }

        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow selectedRow = dataGridView3.Rows[e.RowIndex];
            int i = (int)selectedRow.Cells[0].Value;
            _idRegisters = i;
            GetOneData("Registers", i);

        }

        private async void button9_Click(object sender, EventArgs e)//удаление регистра
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);

            List<string> _registers = new List<string>();
            _registers.Add(_idRegisters.ToString());

            RequestsClass request = new RequestsClass("DeleteData", "Registers", _registers);

            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);




            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, "Registers");
            }
            catch (JsonException ex)
            {

            }
            textBox5.Text = String.Empty;
            textBox6.Text = String.Empty;
            richTextBox1.Clear();
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;

        }

        private async void button10_Click(object sender, EventArgs e)//добавление регистра 
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);

            Registers _register = new Registers(_idDevices, textBox5.Text, textBox6.Text);
            List<string> _registers = new List<string>();
            _registers.Add(_register.Id.ToString());
            _registers.Add(_register.DeviceId.ToString());
            _registers.Add(_register.Name.ToString());
            _registers.Add(_register.Description.ToString());
            _registers.Add(_register.EditingDate.ToString());

            RequestsClass request = new RequestsClass("AddData", "Registers", _registers);

            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);




            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, "Registers");
            }
            catch (JsonException ex)
            {

            }
            textBox5.Text = String.Empty;
            textBox6.Text = String.Empty;
            richTextBox1.Clear();
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;
        }

        private async void button12_Click(object sender, EventArgs e)//сохранение регистра
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8889);

            List<string> _register = new List<string>();
            _register.Add(_idRegisters.ToString());
            _register.Add(_idDevices.ToString());
            _register.Add(textBox5.Text);
            _register.Add(textBox6.Text);
            RequestsClass request = new RequestsClass("UpdateData", "Registers", _register);

            var option = new JsonSerializerOptions
            {
                WriteIndented = false // без форматирования
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(request, option);
            Console.WriteLine($" Отправляемый JSON: {json}");

            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);




            // Читаем ответ
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response_string = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // УДАЛЯЕМ BOM СИМВОЛ (﻿)
            response_string = response_string.Trim('\uFEFF', '\u200B', '\0', ' ', '\t', '\n', '\r');



            // Проверяем первые символы
            if (response_string.Length > 0)
            {
                Console.WriteLine($"Первый символ: '{(int)response_string[0]:X}'");
            }

            //Десериализуем
            try
            {
                await AddDataToDataGridView(response_string, "Registers");
            }
            catch (JsonException ex)
            {

            }

            textBox5.Text = String.Empty;
            textBox6.Text = String.Empty;
            richTextBox1.Clear();
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (_listRegisterValues.Count > 0)
            {
                DateTime startDate = dateTimePicker2.Value.Date; //от
                DateTime endDate = dateTimePicker1.Value.Date.AddDays(1).AddSeconds(-1); ; //до

                var filteredValues = new List<RegisterValues>();

                foreach (var item in _listRegisterValues)
                {
                    if (item is JsonElement valueElement)
                    {
                        try
                        {
                            var registerValue = JsonSerializer.Deserialize<RegisterValues>(valueElement.GetRawText());

                            // ТЕПЕРЬ можно обращаться к Timestamp
                            if (registerValue.Timestamp >= startDate && registerValue.Timestamp <= endDate)
                            {
                                filteredValues.Add(registerValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка фильтрации: {ex.Message}");
                        }
                    }
                }

                // Сортируем и выводим
                filteredValues = filteredValues.OrderBy(v => v.Timestamp).ToList();

                richTextBox1.Clear();
                string lastValue = "";
                for (int i = 0; i < filteredValues.Count; i++)
                {

                    try
                    {


                        richTextBox1.AppendText($"({filteredValues[i].Timestamp:dd.MM.yyyy HH:mm:ss})  Значение: {filteredValues[i].Value}\n");
                        lastValue = filteredValues[i].Value.ToString();
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.AppendText($"Ошибка чтения значения {i + 1}: {ex.Message}\n");
                    }

                }

                if (!filteredValues.Any())
                {
                    richTextBox1.AppendText("Нет данных за выбранный период\n");
                }
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (_registerValuesList.Count >= 200)
            {
                // Блокируем на уровне всего процесса отправки
                if (_isSending) return;
                _isSending = true;
                timer1.Stop();

                try
                {
                    Console.WriteLine($"📤 Последовательная отправка {_registerValuesList.Count} значений...");

                    // Разбиваем на пачки
                    var batches = _registerValuesList
                        .Select((value, index) => new { value, index })
                        .GroupBy(x => x.index / 50)
                        .Select(g => g.Select(x => x.value).ToList())
                        .ToList();

                    // Отправляем последовательно
                    foreach (var batch in batches)
                    {
                        await SendBatchToServer(batch);
                        await Task.Delay(100);
                    }

                    _registerValuesList.Clear();
                    Console.WriteLine($"✅ Все {batches.Count} пачек отправлены");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка отправки: {ex.Message}");
                }
                finally
                {
                    _isSending = false;
                    timer1.Start();
                }
            }
            else
            {
                Random rnd = new Random();
                int i = rnd.Next(0, _backListRegisters.Count);
                float value = rnd.NextSingle() * 1000;
                float rounded_value = (float)Math.Round(value, 2);
                _registerValuesList.Add(new RegisterValues(_backListRegisters[i].Id, rounded_value));
            }
        }

        private async Task SendBatchToServer(List<RegisterValues> batch)
        {
            // УБИРАЕМ проверку _isSending здесь - она уже на уровне timer1_Tick
            try
            {
                var requestData = new List<object>();
                foreach (var value in batch)
                {
                    requestData.Add(new List<object> { value.RegisterId, value.Value });
                }

                var request = new RequestsClass("AddData", "RegisterValues", requestData);
                string json = JsonSerializer.Serialize(request);
                var data = Encoding.UTF8.GetBytes(json);

                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("localhost", 8889);
                var stream = tcpClient.GetStream();

                // Отправляем запрос
                await stream.WriteAsync(data, 0, data.Length);

                // Ждем ответ от сервера
                var buffer = new byte[4096];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"✅ Отправлена пачка из {batch.Count} значений. Ответ: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка отправки пачки: {ex.Message}");
                throw; // Пробрасываем исключение наверх
            }
        }

        private async void timer2_Tick(object sender, EventArgs e)
        {
            _numTick++;
            label12.Text = $"Таймер: {_numTick}с.";
        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Очищаем Panel
            g.Clear(panel1.BackColor);

            // Рисуем все устройства из списка
            foreach (var device in _devicesList)
            {
                DrawDevice(g, device);
            }
        }
    }
}
