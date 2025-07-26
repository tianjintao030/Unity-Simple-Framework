using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using tjtFramework.Singleton;
using tjtFramework.Utiliy;

namespace tjtFramework.Msg
{
    /// <summary>
    /// 消息广播系统
    /// </summary>
    public class MsgSystem : MonoSingleton<MsgSystem>
    {
        [System.Serializable]
        public class MsgReciever : ListWithPriority
        {
            public string name = "";
            public UnityAction<object[]> callback;

            public MsgReciever(string msg_name, int msg_priority, UnityAction<object[]> msg_callback)
            {
                name = msg_name;
                priority = msg_priority;
                callback = msg_callback;
            }
        }

        public Dictionary<string, List<MsgReciever>> msg_recievers = new Dictionary<string, List<MsgReciever>>();


        public void RegistMsgAction(string msg_name, UnityAction<object[]> msg_action, string reciever_name = "", int reciever_priority = 0)
        {
            if (msg_recievers.ContainsKey(msg_name) == false)
                msg_recievers.Add(msg_name, new List<MsgReciever>());

            ListUtility.AddItemWithPriority(msg_recievers[msg_name], new MsgReciever(reciever_name, reciever_priority, msg_action));

            OnMsgRegisted(msg_name, msg_action);
        }


        public void RemoveMsgAction(string msg_name, UnityAction<object[]> msg_action)
        {
            if (msg_recievers.ContainsKey(msg_name) == false)
                return;

            List<MsgReciever> recievers_list = msg_recievers[msg_name];

            for (int i = recievers_list.Count - 1; i >= 0; i--)
                if (recievers_list[i].callback == msg_action)
                    recievers_list.RemoveAt(i);
        }



        public void RemoveMsgAction(string msg_name, string reciever_name)
        {
            if (msg_recievers.ContainsKey(msg_name) == false)
                return;

            List<MsgReciever> recievers_list = msg_recievers[msg_name];

            for (int i = recievers_list.Count - 1; i >= 0; i--)
                if (recievers_list[i].name == reciever_name)
                    recievers_list.RemoveAt(i);
        }




        public void MsgBroadcast(string msg_name, object[] msg_content)
        {
            if (msg_recievers.ContainsKey(msg_name) == false)
                return;

            List<MsgReciever> recievers_list = msg_recievers[msg_name];

            for (int i = 0; i < recievers_list.Count; i++)
                if (recievers_list[i].callback != null) recievers_list[i].callback(msg_content);
        }




        public void ClearAllMsg()
        {
            msg_recievers.Clear();
        }












        [System.Serializable]
        public class MailMsg
        {
            public string msg_name = "";
            public object[] msg_content;

            public float time_to_delete_after_read = 1.0f;

            public bool is_readed = false;
            public float read_time = 0.0f;
        }

        public List<MailMsg> mail_msgs = new List<MailMsg>();


        void Update()
        {
            for (int i = mail_msgs.Count - 1; i >= 0; i--)
                if (mail_msgs[i].is_readed == true && Time.time - mail_msgs[i].read_time > mail_msgs[i].time_to_delete_after_read)
                    mail_msgs.RemoveAt(i);
        }


        public void OnMsgRegisted(string msg_name, UnityAction<object[]> msg_action)
        {
            for (int i = 0; i < mail_msgs.Count; i++)
                if (mail_msgs[i].msg_name == msg_name)
                {
                    msg_action(mail_msgs[i].msg_content);

                    OnMailMsgRead(mail_msgs[i]);
                }
        }

        public void OnMailMsgRead(MailMsg mail_msg)
        {
            if (mail_msg.is_readed == false)
            {
                mail_msg.is_readed = true;
                mail_msg.read_time = Time.time;
            }
        }


        public void MsgMail(string msg_name, object[] msg_content, float time_to_delete_after_read = 1.0f)
        {
            MailMsg msg = new MailMsg();
            msg.msg_name = msg_name;
            msg.msg_content = msg_content;

            msg.time_to_delete_after_read = time_to_delete_after_read;
            msg.is_readed = false;

            mail_msgs.Add(msg);

            if (msg_recievers.ContainsKey(msg_name) == true && msg_recievers[msg_name].Count > 0)
            {
                MsgBroadcast(msg_name, msg_content);
                OnMailMsgRead(msg);
            }
        }


        public void ClearMailMsg(string msg_name)
        {
            for (int i = mail_msgs.Count - 1; i >= 0; i--)
                if (mail_msgs[i].msg_name == msg_name)
                    mail_msgs.RemoveAt(i);
        }

        public void ClearAllMailMsg()
        {
            mail_msgs.Clear();
        }











    }
}


