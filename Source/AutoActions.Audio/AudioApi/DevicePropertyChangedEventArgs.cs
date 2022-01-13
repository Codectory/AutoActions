using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AudioSwitcher.AudioApi
{
    public class DevicePropertyChangedEventArgs : DeviceChangedEventArgs
    {
        public string PropertyName { get; private set; }

        public DevicePropertyChangedEventArgs(IDevice dev, string propertyName = null)
            : base(dev, AudioDeviceEventType.PropertyChanged)
        {
            PropertyName = propertyName;
        }

        private static string GetName(Expression<Func<IDevice, object>> exp)
        {
            var body = exp.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }

        public static DevicePropertyChangedEventArgs FromExpression(IDevice dev, Expression<Func<IDevice, object>> propertyNameExpression)
        {
            return new DevicePropertyChangedEventArgs(dev, GetName(propertyNameExpression));
        }
    }
}
