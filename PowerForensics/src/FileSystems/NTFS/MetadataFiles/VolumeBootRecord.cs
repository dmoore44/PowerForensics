﻿using System;
using System.IO;
using System.Text;

namespace PowerForensics.Ntfs
{
    #region VolumeBootRecordClass

    public class VolumeBootRecord : PowerForensics.Generic.VolumeBootRecord
    {
        #region Properties

        public readonly long BytesPerFileRecord;
        public readonly long BytesPerIndexBlock;
        public readonly long TotalSectors;
        public readonly long MftStartIndex;
        public readonly long MftMirrStartIndex;
        public readonly string VolumeSerialNumber;

        #endregion Properties

        #region Constructors

        internal VolumeBootRecord(byte[] bytes)
        {
            // Check if NTFS Partition
            if (Encoding.ASCII.GetString(bytes, 0x03, 0x08) == "NTFS    ")
            {
                BytesPerSector = BitConverter.ToUInt16(bytes, 0x0B);
                SectorsPerCluster = bytes[0x0D];
                BytesPerCluster = BytesPerSector * SectorsPerCluster;
                ReservedSectors = BitConverter.ToUInt16(bytes, 0x0E);
                MediaDescriptor = (MEDIA_DESCRIPTOR)bytes[0x15];
                SectorsPerTrack = BitConverter.ToUInt16(bytes, 0x18);
                NumberOfHeads = BitConverter.ToUInt16(bytes, 0x1A);
                HiddenSectors = BitConverter.ToUInt32(bytes, 0x1C);
                TotalSectors = BitConverter.ToInt64(bytes, 0x28);
                MftStartIndex = BitConverter.ToInt64(bytes, 0x30);
                MftMirrStartIndex = BitConverter.ToInt64(bytes, 0x38);
                BytesPerFileRecord = getBytesPerFileRecord(bytes, BytesPerCluster);
                BytesPerIndexBlock = getBytesPerIndexBlock(bytes, BytesPerCluster);
                VolumeSerialNumber = getVolumeSerialNumber(bytes);
                CodeSection = Helper.GetSubArray(bytes, 0x50, 0x1AE);
            }
            else
            {
                throw new Exception("Volume is not NTFS formatted.");
            }
        }

        #endregion Constructors

        #region StaticMethods

        #region Get

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static VolumeBootRecord Get(string volume)
        {
            Helper.getVolumeName(ref volume);
            return new VolumeBootRecord(GetBytes(volume));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static VolumeBootRecord GetByPath(string path)
        {
            return new VolumeBootRecord(GetBytesByPath(path));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamToRead"></param>
        /// <returns></returns>
        internal static VolumeBootRecord Get(FileStream streamToRead)
        {
            return new VolumeBootRecord(GetBytes(streamToRead));
        }

        #endregion Get

        #region GetBytes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string volume)
        {
            Helper.getVolumeName(ref volume);
            return Helper.readDrive(volume, 0x00, 0x200);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamToRead"></param>
        /// <returns></returns>
        private static byte[] GetBytes(FileStream streamToRead)
        {
            return Helper.readDrive(streamToRead, 0x00, 0x200);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] GetBytesByPath(string path)
        {
            FileRecord record = FileRecord.Get(path, true);
            return record.GetContent();
        }

        #endregion GetBytes

        #endregion StaticMethods

        #region PrivateMethods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="bytesPerCluster"></param>
        /// <returns></returns>
        private static long getBytesPerFileRecord(byte[] bytes, int bytesPerCluster)
        {
            sbyte clustersPerFileRecord = (sbyte)bytes[0x40];
            if (clustersPerFileRecord < 0)
            {
                return (long)Math.Pow(2, Math.Abs(clustersPerFileRecord));
            }
            else
            {
                return clustersPerFileRecord * bytesPerCluster;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="bytesPerCluster"></param>
        /// <returns></returns>
        private static long getBytesPerIndexBlock(byte[] bytes, int bytesPerCluster)
        {
            sbyte clustersPerIndexBlock = (sbyte)bytes[0x44];
            if (clustersPerIndexBlock < 0)
            {
                return (long)Math.Pow(2, Math.Abs(clustersPerIndexBlock));
            }
            else
            {
                return clustersPerIndexBlock * bytesPerCluster;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string getVolumeSerialNumber(byte[] bytes)
        {
            byte[] serialNumberBytes = Helper.GetSubArray(bytes, 0x48, 0x04);
            Array.Reverse(serialNumberBytes);
            return BitConverter.ToString(serialNumberBytes).Remove(2, 1).Remove(7, 1);
        }

        #endregion PrivateMethods
    }

    #endregion VolumeBootRecordClass
}
