using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientWinForm.DataClasses
{
    public class Interfaces
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } //уникальный идентификатор

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;//название интерфейса

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;//описание интерфейса

        [Required]
        public DateTime EditingDate { get; set; } = DateTime.UtcNow;//последняя дата изменения/создания

        // Конструкторы
        public Interfaces() { }

        public Interfaces(string name, string description)
        {
            Name = name;
            Description = description;
            EditingDate = DateTime.UtcNow;
        }

        public Interfaces(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
            EditingDate = DateTime.UtcNow;
        }

    }

    public class Devices
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // уникальный идентификатор

        [Required]
        public int InterfaceId { get; set; } // ссылка на интерфейс

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // название устройства

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // описание устройства

        [Required]
        public bool IsEnabled { get; set; } = true; // состояние «включено/выключено»

        [Required]
        public DateTime EditingDate { get; set; } = DateTime.UtcNow; // последняя дата изменения/создания

        [Required]
        [MaxLength(50)]
        public string FigureType { get; set; } = "Circle"; // тип фигуры – круг/квадрат/линия

        [Required]
        public int Size { get; set; } = 50; // размер фигуры

        [Required]
        public int PosX { get; set; } = 0; // позиция по оси Х

        [Required]
        public int PosY { get; set; } = 0; // позиция по оси У

        [Required]
        [MaxLength(50)]
        public string Color { get; set; } = "#000000"; // цвет фигуры

        [ForeignKey("InterfaceId")]
        public virtual Interfaces Interface { get; set; }
        // Конструкторы
        public Devices() { }

        public Devices(int interfaceId, string name, string description, bool isEnabled,
                       string figureType, int size, int posX, int posY, string color)
        {
            InterfaceId = interfaceId;
            Name = name;
            Description = description;
            IsEnabled = isEnabled;
            FigureType = figureType;
            Size = size;
            PosX = posX;
            PosY = posY;
            Color = color;
            EditingDate = DateTime.UtcNow;
        }

        public Devices(int id, int interfaceId, string name, string description, bool isEnabled,
                       string figureType, int size, int posX, int posY, string color)
        {
            Id = id;
            InterfaceId = interfaceId;
            Name = name;
            Description = description;
            IsEnabled = isEnabled;
            FigureType = figureType;
            Size = size;
            PosX = posX;
            PosY = posY;
            Color = color;
            EditingDate = DateTime.UtcNow;
        }
    }


    public class Registers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // уникальный идентификатор

        [Required]
        public int DeviceId { get; set; } // ссылка на устройство

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // название регистра

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // описание регистра

        [Required]
        public DateTime EditingDate { get; set; } = DateTime.UtcNow; // последняя дата изменения/создания

        [ForeignKey("DeviceId")]
        public virtual Devices Device { get; set; }

        // Конструкторы
        public Registers() { }

        public Registers(int deviceId, string name, string description)
        {
            DeviceId = deviceId;
            Name = name;
            Description = description;
            EditingDate = DateTime.UtcNow;
        }

        public Registers(int id, int deviceId, string name, string description)
        {
            Id = id;
            DeviceId = deviceId;
            Name = name;
            Description = description;
            EditingDate = DateTime.UtcNow;
        }
    }


    public class RegisterValues
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // уникальный идентификатор

        [Required]
        public int RegisterId { get; set; } // ссылка на регистр

        [Required]
        public float Value { get; set; } // значение регистра

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // время записи значения

        [ForeignKey("RegisterId")]
        public virtual Registers Register { get; set; }

        // Конструкторы
        public RegisterValues() { }

        public RegisterValues(int registerId, float value)
        {
            RegisterId = registerId;
            Value = value;
            Timestamp = DateTime.UtcNow;
        }

        public RegisterValues(int registerId, float value, DateTime timestamp)
        {
            RegisterId = registerId;
            Value = value;
            Timestamp = timestamp;
        }
    }

    public class Logs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // уникальный идентификатор

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // время записи сообщения

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty; // сообщение

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "Info"; // тип сообщения

        // Конструкторы
        public Logs() { }

        public Logs(string message, string type)
        {
            Message = message;
            Type = type;
            Timestamp = DateTime.UtcNow;
        }

        public Logs(string message, string type, DateTime timestamp)
        {
            Message = message;
            Type = type;
            Timestamp = timestamp;
        }
    }
    class RequestsClass
    {
        private string _nameRequests = string.Empty; //название запроса
        private string _typeObjects = string.Empty; //название объекта
        private List<string> _objectsList = new List<string>(); //параметры объекста
        private List<object> _objectList1 = new List<object>(); //объекты
        public string NameRequests
        {
            get { return _nameRequests; }
            set { _nameRequests = value; }
        }

        public string TypeObjects
        {
            get { return _typeObjects; }
            set { _typeObjects = value; }
        }

        public List<string> ObjectsList
        {
            get { return _objectsList; }
            set { _objectsList = value; }
        }
        public List<object> ObjectList1
        {
            get { return _objectList1; }
            set { _objectList1 = value; }
        }

        public RequestsClass(string nameRequests, string typeObjects, List<object> objectsList1)
        {
            this.NameRequests = nameRequests;
            this.TypeObjects = typeObjects;
            this.ObjectList1 = objectsList1;
        }
        public RequestsClass(string nameRequests, string typeObjects, List<string> objectsList)
        {
            this.NameRequests = nameRequests;
            this.TypeObjects = typeObjects;
            this.ObjectsList = objectsList;
        }
    }

    class ResponseGetAllData
    {
        private List<Interfaces> _objectListInterfaces = new List<Interfaces>();
        private List<Devices> _objectListDevices = new List<Devices>();
        private List<Registers> _objectListRegisters = new List<Registers>();
        private List<RegisterValues> _objectListRegisterValues = new List<RegisterValues>();
        private List<Logs> _objectListLogs = new List<Logs>();

        public List<Interfaces> ObjectsListInterfaces
        {
            get { return _objectListInterfaces; }
            set { _objectListInterfaces = value; }
        }

        public List<Devices> ObjectsListDevices
        {
            get { return _objectListDevices; }
            set { _objectListDevices = value; }
        }

        public List<Registers> ObjectsListRegisters
        {
            get { return _objectListRegisters; }
            set { _objectListRegisters = value; }
        }

        public List<RegisterValues> ObjectsListRegisterValues
        {
            get { return _objectListRegisterValues; }
            set { _objectListRegisterValues = value; }
        }

        public List<Logs> ObjectsListLogs
        {
            get { return _objectListLogs; }
            set { _objectListLogs = value; }
        }

        // Конструктор по умолчанию
        public ResponseGetAllData()
        {
        }

        // Существующий конструктор с параметрами
        public ResponseGetAllData(List<Interfaces> objectsListInterfaces, List<Devices> objectListDevices)
        {
            this.ObjectsListInterfaces = objectsListInterfaces;
            this.ObjectsListDevices = objectListDevices;
        }

        // Новый конструктор для всех типов
        public ResponseGetAllData(
            List<Interfaces> objectsListInterfaces,
            List<Devices> objectListDevices,
            List<Registers> objectListRegisters,
            List<RegisterValues> objectListRegisterValues,
            List<Logs> objectListLogs)
        {
            this.ObjectsListInterfaces = objectsListInterfaces;
            this.ObjectsListDevices = objectListDevices;
            this.ObjectsListRegisters = objectListRegisters;
            this.ObjectsListRegisterValues = objectListRegisterValues;
            this.ObjectsListLogs = objectListLogs;
        }
    }
    class ResponseGetOneItem
    {
        private string _typeObjects = String.Empty;
        private object _item = new object();
        private List<object> _objectsListRegistrsValue = new List<object>();//кесли возвращаем значения регистра
        public string TypeObjects
        {
            get { return _typeObjects; }
            set { _typeObjects = value; }
        }

        public object Item
        {
            get { return _item; }
            set { _item = value; }
        }
        public List<object> ObjectListRegistrsValue
        {
            get { return _objectsListRegistrsValue; }
            set { _objectsListRegistrsValue = value; }
        }
        public ResponseGetOneItem(string typeObjects, object item)//дефолтный конструктор , который используется в 90% случаев
        {
            this.TypeObjects = typeObjects;
            this.Item = item;
        }

        public ResponseGetOneItem()
        {

        }

        public ResponseGetOneItem(string typeObjects, object item, List<object> objectListRegistersValue)//конструктор для возврата значений регистра;
        {
            this.TypeObjects = typeObjects;
            this.Item = item;
            this.ObjectListRegistrsValue = objectListRegistersValue;
        }
    }
    

}
