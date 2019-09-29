using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DiagramDesigner.Annotations;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.Model
{
    public class ConnectionModel : NotifyPropertyChangedBase, IXmlSerializable
    {

        public ConnectionModel([NotNull] ConnectorModel sinkConnector, [NotNull] ConnectorModel sourceConnector) : this(sinkConnector, sourceConnector, Guid.NewGuid())
        {
        }
        public ConnectionModel([NotNull] ConnectorModel sinkConnector, [NotNull] ConnectorModel sourceConnector, Guid id)
        {
            SinkConnector = sinkConnector ?? throw new ArgumentNullException(nameof(sinkConnector));
            SourceConnector = sourceConnector ?? throw new ArgumentNullException(nameof(sourceConnector));
            ID = id;
        }

        #region ID Property

        private Guid _id;
        public Guid ID
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        #endregion

        #region SinkConnector Property

        private ConnectorModel _sinkConnector;
        public ConnectorModel SinkConnector
        {
            get => _sinkConnector;
            set => _sinkConnector = value ?? throw new Exception("SinkConnector can't be null");
        }

        #endregion

        #region SourceConnector Property

        private ConnectorModel _sourceConnector;
        public ConnectorModel SourceConnector
        {
            get => _sourceConnector;
            set => _sourceConnector = value ?? throw new Exception("SourceConnector can't be null");
        }

        #endregion

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Connection");
            writer.WriteElementString("ID", ID.ToString());
            writer.WriteElementString("SourceID", SourceConnector.ID.ToString());
            writer.WriteElementString("SinkID", SinkConnector.ID.ToString());
            writer.WriteEndElement();
        }
    }
}