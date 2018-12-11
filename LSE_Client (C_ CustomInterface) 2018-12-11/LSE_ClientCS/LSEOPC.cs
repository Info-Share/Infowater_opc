using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
//Need "OPC Core Components" from OPC Foundation 
using OpcRcw.Da;
using OpcRcw.Comn;

namespace LSE_ClientCS
{
    public class LSEOPC : IOPCDataCallback 
    {
        public object[] Values;
        public int iItemCount = 4;

        public void LSE_Connect()
        {
            IOPCServer m_OPCServer;
            IOPCGroupStateMgt2 m_OPCGroup2;
            IOPCItemMgt m_OPCItem;
            IConnectionPointContainer m_OPCConnPointCntnr;
            IConnectionPoint m_OPCConnPoint;

            //---------Connect LSEOPC Server

            Type typeofOPCserver = Type.GetTypeFromProgID("Intellution.LSEOPC");
            m_OPCServer = (IOPCServer)Activator.CreateInstance(typeofOPCserver);
              
            //----------Add Group for DA3.0

            int m_iServerGroup;
            int iRevisedUpdateRate;
            Type typGrpMgt = typeof(IOPCGroupStateMgt2);
            Guid guidGroupStateMgt = typGrpMgt.GUID;
            object group = null;

            m_OPCServer.AddGroup("Group1", 1, 1000, 0, IntPtr.Zero, IntPtr.Zero, 0, out m_iServerGroup, out iRevisedUpdateRate, ref guidGroupStateMgt, out group);
            m_OPCGroup2 = (IOPCGroupStateMgt2)group;

            //------------Add items

            string[] ItemName = new string[iItemCount];
            ItemName[0] = "Device0:D00000";
            ItemName[1] = "Device0:D00001";
            ItemName[2] = "Device0:D00002";
            ItemName[3] = "Device0:D00003";
            //ItemName[4] = "Device0:D00004";
            //ItemName[5] = "Device0:D00005";
            //ItemName[6] = "Device0:D00006";
            //ItemName[7] = "Device0:D00007";
            //ItemName[8] = "Device0:D00008";
            //ItemName[9] = "Device0:D00009";
            //ItemName[10] = "Device0:D00010";
            //ItemName[11] = "Device0:D00011";
            //ItemName[12] = "Device0:D00012";
            //ItemName[13] = "Device0:D00013";
            //ItemName[14] = "Device0:D00014";
            //ItemName[15] = "Device0:D00015";

            OPCITEMDEF[] itemDef = new OPCITEMDEF[iItemCount];

            for (int i = 0; i < iItemCount; i++)
            {
                itemDef[i].szItemID = ItemName[i];
                itemDef[i].bActive = 1;
                itemDef[i].hClient = i;
            }

            m_OPCItem = (IOPCItemMgt)m_OPCGroup2;

            IntPtr ppResult;
            IntPtr ppErrors;

            m_OPCItem.AddItems(iItemCount, itemDef, out ppResult, out ppErrors);

            //----------Sync Read items

            IOPCSyncIO2 m_Sync = (IOPCSyncIO2)m_OPCGroup2;   //for DA3.0
            
            OPCITEMRESULT itemResult;
            int[] errors = new int[iItemCount];
            int[] ServerHd = new int[iItemCount];
            IntPtr posRes = ppResult;

            for (int i = 0; i < iItemCount; i++)
            {
                itemResult = (OPCITEMRESULT)Marshal.PtrToStructure(posRes, typeof(OPCITEMRESULT));
                if (errors[i] == 0)
                {
                    ServerHd[i] = itemResult.hServer;
                }
                Marshal.DestroyStructure(posRes, typeof(OPCITEMRESULT));
                posRes = (IntPtr)(posRes.ToInt32() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMRESULT)));
            }

            IntPtr ppItemVal;
            m_Sync.Read(OPCDATASOURCE.OPC_DS_DEVICE, iItemCount, ServerHd, out ppItemVal, out ppErrors);
            
            //--------------Read Data values

            IntPtr posItem;
			OPCITEMSTATE ItemState;
            Values = new object[iItemCount];

            Marshal.Copy(ppErrors, errors, 0, iItemCount);
			posItem = ppItemVal;

            try
            {
                for (int i = 0; i < iItemCount; i++)
                {
                    ItemState = (OPCITEMSTATE)Marshal.PtrToStructure(posItem, typeof(OPCITEMSTATE));
                    if (errors[i] == 0)
                    {
                        Values[i] = ItemState.vDataValue;
                        //TimeStamps[i] = ItemState.ftTimeStamp;
                        //Qualities[i] = ItemState.wQuality;
                    }
                    Marshal.DestroyStructure(posItem, typeof(OPCITEMSTATE));
                    posItem = (IntPtr)(posItem.ToInt32() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMSTATE)));
                }
            }
            catch
            {
                MessageBox.Show("Please confirm LSEOPC Server and Address (Devicename:Address)...");
                Marshal.FreeCoTaskMem(ppItemVal);
                Marshal.FreeCoTaskMem(ppErrors);
                iItemCount = 0;
                return;
            }
			Marshal.FreeCoTaskMem(ppItemVal);
            Marshal.FreeCoTaskMem(ppErrors);

            //------------Async Read items DA3.0 after Sync Read
            
            int iKeepAliveTime = 10000;
            m_OPCGroup2.SetKeepAlive(iKeepAliveTime, out iKeepAliveTime);

            //Add for auto refresh Async read
            m_OPCConnPointCntnr = (IConnectionPointContainer)m_OPCGroup2;
            Guid guidDataCallback = Marshal.GenerateGuidForType(typeof(IOPCDataCallback));
            m_OPCConnPointCntnr.FindConnectionPoint(ref guidDataCallback, out m_OPCConnPoint);

            int m_iCallBackConnection;

            //Async Read Callback Auto refresh set
            m_OPCConnPoint.Advise((LSEOPC) this, out m_iCallBackConnection);

            int wCancelID = 0;

            IOPCAsyncIO3 m_Async = (IOPCAsyncIO3)m_OPCGroup2;
            m_Async.Read(iItemCount, ServerHd, 1234567, out wCancelID, out ppErrors);

            return;

        }

        //------------ IOPCDataCallback subfunctions

        public void OnDataChange(
            int dwTransid,
            int hGroup,
            int hrMasterquality,
            int hrMastererror,
            int dwCount,
            int[] phClientItems,
            object[] pvValues,
            short[] pwQualities,
            OpcRcw.Da.FILETIME[] pftTimeStamps,
            int[] pErrors)
        {
            //MessageBox.Show("Data Changed....");
            for (int i = 0; i < dwCount; i++)
            {
                Values[phClientItems[i]] = pvValues[i];
                //TimeStamps[phClientItems[i]] = pftTimeStamps[i];
                //Qualities[phClientItems[i]] = pwQualities[i];
            }
        }

        public void OnReadComplete(
            int dwTransid,
            int hGroup,
            int hrMasterquality,
            int hrMastererror,
            int dwCount,
            int[] phClientItems,
            object[] pvValues,
            short[] pwQualities,
            OpcRcw.Da.FILETIME[] pftTimeStamps,
            int[] pErrors)
        {
            //MessageBox.Show("OnReadComplete....");
        }

        public void OnWriteComplete(
            int dwTransid,
            int hGroup,
            int hrMastererror,
            int dwCount,
            int[] phClientItems,
            int[] pErrors)
        {
            //MessageBox.Show("OnWriteComplete....");
        }

        public void OnCancelComplete(
            int dwTransid,
            int hGroup)
        {
            //MessageBox.Show("OnCancelComplete....");
        }

    }
}
