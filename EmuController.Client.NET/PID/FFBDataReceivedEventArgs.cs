// Copyright 2020 Maris Melbardis
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
