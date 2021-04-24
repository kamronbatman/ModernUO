/*************************************************************************
 * ModernUO                                                              *
 * Copyright 2019-2020 - ModernUO Development Team                       *
 * Email: hi@modernuo.com                                                *
 * File: IGenericReader.cs                                               *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Net;

namespace Server
{
    public interface IGenericReader
    {
        string ReadString();
        DateTime ReadDateTime();
        TimeSpan ReadTimeSpan();
        DateTime ReadDeltaTime();
        decimal ReadDecimal();
        long ReadLong();
        ulong ReadULong();
        int ReadInt();
        uint ReadUInt();
        short ReadShort();
        ushort ReadUShort();
        double ReadDouble();
        float ReadFloat();
        byte ReadByte();
        sbyte ReadSByte();
        bool ReadBool();
        int ReadEncodedInt();
        IPAddress ReadIPAddress();
        Point3D ReadPoint3D();
        Point2D ReadPoint2D();
        Rectangle2D ReadRect2D();
        Rectangle3D ReadRect3D();
        Map ReadMap();
        T ReadEntity<T>() where T : class, ISerializable;
        List<T> ReadEntityList<T>() where T : class, ISerializable;
        HashSet<T> ReadEntitySet<T>() where T : class, ISerializable;
        Race ReadRace();
        int Read(Span<byte> buffer);
        T ReadEnum<T>() where T : unmanaged, Enum;
    }
}
