using System;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    /// <summary>
    ///     PROPERTYKEY is defined in wtypes.h
    /// </summary>
    public struct PropertyKey
    {
        /// <summary>
        ///     Format ID
        /// </summary>
        public readonly Guid FormatId;

        /// <summary>
        ///     Property ID
        /// </summary>
        public readonly int PropertyId;

        /// <summary>
        ///     <param name="formatId"></param>
        ///     <param name="propertyId"></param>
        /// </summary>
        public PropertyKey(Guid formatId, int propertyId)
        {
            FormatId = formatId;
            PropertyId = propertyId;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PropertyKey))
                return false;

            PropertyKey key = (PropertyKey)obj;

            return key.FormatId == this.FormatId && key.PropertyId == this.PropertyId;
        }

        public static bool operator ==(PropertyKey k1, PropertyKey k2)
        {
            return k1.FormatId == k2.FormatId && k1.PropertyId == k2.PropertyId;
        }

        public static bool operator !=(PropertyKey k1, PropertyKey k2)
        {
            return !(k1 == k2);
        }

        public override int GetHashCode()
        {
            return this.FormatId.GetHashCode() * 17 + this.PropertyId.GetHashCode();
        }
    }
}