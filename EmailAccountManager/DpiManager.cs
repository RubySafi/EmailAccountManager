using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Data;

namespace EmailAccountManager
{
    public class DpiManager
    {

        private PresentationSource source;
        public Matrix LogicalToDeviceMatrix { get; private set; }
        public Matrix DeviceToLogicalMatrix { get; private set; }

        public DpiManager(Visual visual)
        {
            Update(visual);
        }

        public void Update(Visual visual)
        {
            source = PresentationSource.FromVisual(visual);
            if (source?.CompositionTarget != null)
            {
                LogicalToDeviceMatrix = source.CompositionTarget.TransformToDevice;
                DeviceToLogicalMatrix = source.CompositionTarget.TransformFromDevice;
            }
            else
            {
                LogicalToDeviceMatrix = Matrix.Identity;
                DeviceToLogicalMatrix = Matrix.Identity;
            }
        }
    }
}
