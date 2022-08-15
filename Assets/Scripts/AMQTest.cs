using UnityEngine;

using Apache.NMS;
using Apache.NMS.Util;
using System;
using UnityEngine.UI;

public class AMQTest : MonoBehaviour
{
    public string amqUrl = "tcp://localhost:61616/";
    public string amqTopic = "TestTest";

    private IConnectionFactory factory;
    private IConnection connection;
    private ITextMessage message = null;
    private readonly TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);

    private ISession session;
    private IMessageConsumer consumer;
    private IMessageProducer producer;
    private IDestination destination;
    private int reloadTimer;

    void Start()
    {
        factory = new NMSConnectionFactory(amqUrl);
        connection = factory.CreateConnection();

        session = connection.CreateSession();
        destination = SessionUtil.GetDestination(session, "topic://" + amqTopic);
        consumer = session.CreateConsumer(destination);

        producer = session.CreateProducer(destination);
        producer.DeliveryMode = MsgDeliveryMode.Persistent;
        producer.RequestTimeout = receiveTimeout;

        try
        {
            connection.Start();
            consumer.Listener += new MessageListener(OnMessage);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message + " - " + e.GetBaseException().ToString());
        }
    }

    void FixedUpdate()
    {
        reloadTimer++;
        if (reloadTimer > 60)
        {
            reloadTimer = 0;
            SendTestMessage("Hello world");
        }
    }

    void SendTestMessage(string message)
    {
        try
        {
            ITextMessage request = session.CreateTextMessage(message);
            request.NMSCorrelationID = "abc";
            request.Properties["NMSXGroupID"] = "GroupID";
            request.Properties["myHeader"] = "Header";

            producer.Send(request);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message + " - " + e.GetBaseException().ToString());
        }
    }
    void OnMessage(IMessage receivedMsg)
    {
        message = receivedMsg as ITextMessage;
        Debug.Log("Received message with text: " + message.Text);
    }
}
