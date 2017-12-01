using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using System.IO;

using System.Diagnostics;

namespace IQTWcf
{
    #region Implementation of IDispatchMessageInspector

    public class MetadataInspector : IDispatchMessageInspector
    {

        // Assume utf-8, note that Data Services supports
        // charset negotation, so this needs to be more
        // sophisticated (and per-request) if clients will 
        // use multiple charsets
        private static Encoding encoding = Encoding.UTF8;
        private static string mdataresponse = string.Empty; 


        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (request.Properties.ContainsKey("UriTemplateMatchResults"))
            {
                HttpRequestMessageProperty httpmsg = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
                UriTemplateMatch match = (UriTemplateMatch)request.Properties["UriTemplateMatchResults"];

                if (mdataresponse == string.Empty)
                {
                    string mdata = match.RequestUri.PathAndQuery;

                    if (mdata.Contains("$metadata"))
                    {
                        return mdata;
                    }
                }
            }
            return null;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if (correlationState != null && correlationState is string)
            {
                //     if we have a $metadata response then buffer the response 
                //     and merge it with the updated items
                if (mdataresponse == string.Empty)
                {
                    XmlDictionaryReader reader = reply.GetReaderAtBodyContents();
                    reader.ReadStartElement();
                    string content = MetadataInspector.encoding.GetString(reader.ReadContentAsBase64());

                    string filename = ((string)correlationState).Replace(@"/$", ".").Substring(1) + ".xml";
                    string fullpath = AppDomain.CurrentDomain.BaseDirectory + filename;

                    File.WriteAllText(fullpath, content);

                    mdataresponse = content;

                    //mdataresponse = MetadataConverter.Convert(content);
                }

                Message newreply = Message.CreateMessage(MessageVersion.None, "", new Writer(mdataresponse));
                newreply.Properties.CopyProperties(reply.Properties);

                reply = newreply;
            }
        }

        #endregion IDispatchMessageInspector Members

        class Writer : BodyWriter
        {
            private string content;

            public Writer(string content)
                : base(false)
            {
                this.content = content;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("Binary");
                byte[] buffer = MetadataInspector.encoding.GetBytes(this.content);
                writer.WriteBase64(buffer, 0, buffer.Length);
                writer.WriteEndElement();
            }
        }
    }

    #endregion Implementation of IDispatchMessageInspector



    // Simply apply this attribute to a DataService-derived class to get
    // Metadata modification support in that service
    [AttributeUsage(AttributeTargets.Class)]
    public class MetadataSupportBehaviorAttribute : Attribute, IServiceBehavior
    {
        #region IServiceBehavior Members

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher ed in cd.Endpoints)
                {
                    ed.DispatchRuntime.MessageInspectors.Add(new MetadataInspector());
                }
            }
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }

}