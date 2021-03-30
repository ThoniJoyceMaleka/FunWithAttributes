﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _ThoniMaleka__Project_2_FunWithAttributes
{
    class Program
    {
        static void Main(string[] args)
        {
            MyModel model = new MyModel()
            {
                Prop1 = 0,
                Prop2 = 16d,
                Prop3 = "text",
                Prop4 = DateTime.Now,
                ShortCircuitOnInvalid = false
            };

            try
            {
                Console.WriteLine("Initial configuration:");
                if (model.IsValid)
                {
                    Console.WriteLine("Object is valid");
                }
                else
                {
                    Console.WriteLine(model.InvalidPropertyMessage);
                }

                model.Prop4 = model.TriggerDate.AddDays(-1);
                Console.WriteLine("After correcting Prop4 (a DateTime):");
                if (model.IsInvalid)
                {
                    Console.WriteLine("Object is valid");
                }
                else
                {
                    Console.WriteLine(model.InvalidPropertyMessage);
                }
                model.Prop2 = 7d;
                Console.WriteLine("After correcting Prop2 (a double): ");
                if (model.IsValid)
                {
                    Console.WriteLine("Object is valid");
                }
                else
                {
                    Console.WriteLine(model.InvalidPropertyMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }



    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class InvalidValueAttribute : System.Attribute
    {

        public enum TriggerType
        {
            Valid,
            Equal,
            NotEqual,
            Over,
            Under
        };

        public TriggerType Trigger { get; protected set; }

        public object TriggerValue { get; protected set; }

        public Type ExpectedType { get; protected set; }

        public object PropertyValue { get; protected set; }

        public string TriggerMsg
        {
            get
            {
                string format = string.Empty;
                switch (this.Trigger)
                {
                    case TriggerType.Valid:
                    case TriggerType.Equal: format = "equald to"; break;
                    case TriggerType.NotEqual: format = "not equal to"; break;
                    case TriggerType.Over: format = "greater than"; break;
                    case TriggerType.Under: format = "less than"; break;
                }
                if (!string.IsNullOrEmpty(format))
                {
                    format = string.Concat("Cannot be ", format, "'{0}'. \r\n '{1}'. \r\n");

                }
                return (!string.IsNullOrEmpty(format)) ? string.Format(format, this.TriggerValue, this.PropertyValue) : string.Empty;

            }
        }

        public InvalidValueAttribute(object triggerValue, TriggerType trigger = TriggerType.Valid, Type expectedType = null)
        {
            if (this.IsIntrinsic(triggerValue.GetType()))
            {
                this.Trigger = trigger;
                if (expectedType != null)
                {
                    if (this.IsDateTime(expectedType))
                    {
                        long ticks = Math.Min(Math.Max(0, Convert.ToInt64(triggerValue)), Int64.MaxValue);
                        this.TriggerValue = new DateTime(ticks);
                    }
                    else
                    {
                        this.TriggerValue = triggerValue;
                    }
                    this.ExpectedType = expectedType;
                }
                else
                {
                    this.TriggerValue = triggerValue;
                    this.ExpectedType = triggerValue.GetType();
                }
            }
            else
            {
                throw new ArgumentException("The triggerValue parameter must be a primitive, string, or DateTime, and must match the type of attributed property.");
            }
        }

        public bool IsValid(object value)
        {
            bool result = false;

            this.PropertyValue = value;

            Type valueType = value.GetType();

            if (this.IsDateTime(valueType))
            {
                this.TriggerValue = this.MakeNormalizedDateTime();

                this.ExpectedType = typeof(DateTime);
            }

            if (valueType == this.ExpectedType)
            {
                switch(this.Trigger)
                {
                    case TriggerType.Equal: result = this.IsEqual(value, this.TriggerValue); break;
                    case TriggerType.Valid:
                    case TriggerType.NotEqual: result = this.IsNotEqual(value, this.TriggerValue); break;
                    case TriggerType.Over: result = !this.GreaterThan(value, this.TriggerValue); break;
                    case TriggerType.Under: result = !this.LessThan(value, this.TriggerValue); break;

                }
            }
            else
            {
                throw new InvalidOperationException("The property value and trigger value are not of compatible types.");
            }
            return result;
        }

        private DateTime MakeNormalizedDateTime()
        {
            DateTime date = new DateTime(0);
            if (this.IsInteger(this.TriggerValue.GetType()))
            {
                long ticks = Math.Min(Math.Max(0, Convert.ToInt64(this.TriggerValue)), Int64.MaxValue);
                date = new DateTime(ticks);
            }
            else if (this.IsDateTime(this.TriggerValue.GetType()))
            {
                date = Convert.ToDateTime(this.TriggerValue);
            }
            return date;
        }

        protected bool IsUnsignedInteger(Type type)
        {
            return ((type != null) &&
                (type == typeof(uint) ||
                type == typeof(ushort) ||
                type == typeof(ulong)));
        }

        protected bool IsInteger(Type type)
        {
            return ((type != null) &&
                (this.IsUnsignedInteger(type) ||
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(long)));
        }

        protected bool IsDecimal(Type type)
        {
            return (type != null && type == typeof(decimal));
        }

        protected bool IsString(Type type)
        {
            return (type != null && type == typeof(string));
        }

        protected bool IsDateTime(Type type)
        {
            return (type != null && type == typeof(DateTime));
        }

        protected bool IsFloatingPoint(Type type)
        {
            return ((type != null) && (type == typeof(double) || type == typeof(float)));
        }

        protected bool IsIntrinsic(Type type)
        {
            return (this.IsInteger(type) ||
                this.IsDecimal(type) ||
                this.IsFloatingPoint(type) ||
                this.IsString(type) ||
                this.IsDateTime(type)
                );
        }

        protected bool LessThan(object obj1, object obj2)
        {
            bool result = false;
            Type objType = obj1.GetType();
            if (this.IsInteger(objType))
            {
                result = (this.IsUnsignedInteger(objType) && this.IsUnsignedInteger(obj2.GetType())) ?
                    (Convert.ToUInt64(obj1) == Convert.ToUInt64(obj2)) :
                    (Convert.ToUInt64(obj1) == Convert.ToUInt64(obj2));
            }
        }
    }
}