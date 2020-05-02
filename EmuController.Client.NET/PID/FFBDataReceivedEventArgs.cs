using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuController.Client.NET.PID
{

    public class FFBDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Type of FFB data.
        /// </summary>
        public PIDReportIdEnum ReportId { get; }

        /// <summary>
        /// FFB packet data.
        /// </summary>
        public FFBPacket FFBPacket { get; }

        /// <summary>
        /// Creates FFBDataReceivedEventArgs object and populates FFBPacket with data from received message from named pipe server.
        /// </summary>
        /// <param name="buffer">The message received from named pipe server</param>
        public FFBDataReceivedEventArgs(byte[] buffer)
        {

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            ReportId = (PIDReportIdEnum)buffer[0];


            FFBPacket = ReportId switch
            {
                PIDReportIdEnum.SetEffect => new PIDSetEffectReport(buffer),
                PIDReportIdEnum.SetEnvelope => new PIDSetEnvelopeReport(buffer),
                PIDReportIdEnum.SetCondition => new PIDSetConditionReport(buffer),
                PIDReportIdEnum.SetPeriodic => new PIDSetPeriodicReport(buffer),
                PIDReportIdEnum.SetConstant => new PIDSetConstantForce(buffer),
                PIDReportIdEnum.SetRamp => new PIDSetRampReport(buffer),
                PIDReportIdEnum.SetCustomForceData => new PIDSetCustomForceDataReport(buffer),
                PIDReportIdEnum.DownloadSample => new PIDDownloadSampleReport(buffer),
                PIDReportIdEnum.EffectOperation => new PIDEffectOperationReport(buffer),
                PIDReportIdEnum.DeviceControl => new PIDDeviceControlReport(buffer),
                PIDReportIdEnum.BlockFree => new PIDBlockFreeReport(buffer),
                PIDReportIdEnum.SetGain => new PIDSetGainReport(buffer),
                PIDReportIdEnum.SetCustomForce => new PIDSetCustomForceReport(buffer),
                PIDReportIdEnum.NewEffect => new PIDNewEffectReport(buffer),
                PIDReportIdEnum.BlockLoad => new PIDBloadLoadReport(buffer),
                _ => throw new NotSupportedException()
            };

        }

    }
}
