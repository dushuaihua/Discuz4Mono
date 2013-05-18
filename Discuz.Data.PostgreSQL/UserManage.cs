using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using Discuz.Data;
using Discuz.Config;
using Discuz.Common;
using Discuz.Entity;
using Discuz.Common.Generic;
using NpgsqlTypes;
using Npgsql;

namespace Discuz.Data.PostgreSQL
{
    public partial class DataProvider : IDataProvider
    {
        private static int _lastRemoveTimeout;
        private static Dictionary<TrendType, int> stat = new Dictionary<TrendType, int>();
        private static DateTime lastTrendUpdateTime = DateTime.Now;

        static DataProvider()
        {
            Initial();
        }

        private static void Initial()
        {
            stat.Clear();
            stat.Add(TrendType.Login, 0);
            stat.Add(TrendType.Register, 0);
            stat.Add(TrendType.Topic, 0);
            stat.Add(TrendType.Poll, 0);
            stat.Add(TrendType.Bonus, 0);
            stat.Add(TrendType.Debate, 0);
            stat.Add(TrendType.Post, 0);
        }

        public DataTable GetUsers(string idList)
        {
            if (!Utils.IsNumericList(idList))
                return new DataTable();

            string commandText = string.Format("SELECT [uid],[username] FROM [{0}users] WHERE [groupid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                idList);
            return DbHelper.ExecuteDatasetInMasterDB(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetUserGroupInfoByGroupid(int groupId)
        {
            string commandText = string.Format("SELECT TOP 1 {0} FROM  [{1}usergroups] WHERE [groupid]={2}",
                                                DbFields.USER_GROUPS,
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetMedal()
        {
            return DbHelper.ExecuteDataset(CommandType.Text, GetMedalSql()).Tables[0];
        }

        public string GetMedalSql()
        {
            return string.Format("SELECT {0} FROM [{1}medals]", DbFields.MEDALS, BaseConfigs.GetTablePrefix);
        }

        public DataTable GetExistMedalList()
        {
            string commandText = string.Format("SELECT [medalid],[image] FROM [{0}medals] WHERE [image]<>''",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public void AddMedal(string name, int available, string image)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@medalid", (DbType)NpgsqlDbType.Smallint,2, GetMaxMedalId()),
				DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Varchar,50, name),
                DbHelper.MakeInParam("@available", (DbType)NpgsqlDbType.Integer, 4, available),
				DbHelper.MakeInParam("@image",(DbType)NpgsqlDbType.Varchar,30,image)
			};
            string commandText = string.Format("INSERT INTO [{0}medals] (medalid,name,available,image) Values (@medalid,@name,@available,@image)",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateMedal(int medalId, string name, string image)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@medalid", (DbType)NpgsqlDbType.Smallint,2, medalId),
				DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Varchar,50, name),
				DbHelper.MakeInParam("@image",(DbType)NpgsqlDbType.Varchar,30,image)
			};
            string commandText = string.Format("UPDATE [{0}medals] SET [name]=@name,[image]=@image  Where [medalid]=@medalid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void SetAvailableForMedal(int available, string medalIdList)
        {
            DbParameter[] parms = {
				                       DbHelper.MakeInParam("@available", (DbType)NpgsqlDbType.Integer, 4, available)
			                      };
            string commandText = string.Format("UPDATE [{0}medals] SET [available]=@available WHERE [medalid] IN({1})",
                                                BaseConfigs.GetTablePrefix,
                                                medalIdList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        private int GetMaxMedalId()
        {
            string commandText = string.Format("SELECT COALESCE(MAX(medalid), 0) FROM [{0}medals]", BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText)) + 1;
        }

        /// <summary>
        /// ��õ�ָ����������Ϣ
        /// </summary>
        /// <returns>��������Ϣ</returns>
        public DataTable GetAdminGroupList()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}admingroups]",
                                                DbFields.ADMIN_GROUPS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDatasetInMasterDB(CommandType.Text, commandText).Tables[0];
        }

        /// <summary>
        /// ���ù�������Ϣ
        /// </summary>
        /// <param name="__admingroupsInfo">��������Ϣ</param>
        /// <returns>���ļ�¼��</returns>
        public int SetAdminGroupInfo(AdminGroupInfo adminGroupsInfo)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@admingid",(DbType)NpgsqlDbType.Smallint,2,adminGroupsInfo.Admingid),
									   DbHelper.MakeInParam("@alloweditpost",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Alloweditpost),
									   DbHelper.MakeInParam("@alloweditpoll",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Alloweditpoll),
									   DbHelper.MakeInParam("@allowstickthread",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowstickthread),
									   DbHelper.MakeInParam("@allowmodpost",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowmodpost),
									   DbHelper.MakeInParam("@allowdelpost",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowdelpost),
									   DbHelper.MakeInParam("@allowmassprune",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowmassprune),
									   DbHelper.MakeInParam("@allowrefund",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowrefund),
									   DbHelper.MakeInParam("@allowcensorword",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowcensorword),
									   DbHelper.MakeInParam("@allowviewip",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowviewip),
									   DbHelper.MakeInParam("@allowbanip",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowbanip),
									   DbHelper.MakeInParam("@allowedituser",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowedituser),
									   DbHelper.MakeInParam("@allowmoduser",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowmoduser),
									   DbHelper.MakeInParam("@allowbanuser",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowbanuser),
									   DbHelper.MakeInParam("@allowpostannounce",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowpostannounce),
									   DbHelper.MakeInParam("@allowviewlog",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowviewlog),
									   DbHelper.MakeInParam("@disablepostctrl",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Disablepostctrl),
                                       DbHelper.MakeInParam("@allowviewrealname",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowviewrealname)
								   };
            return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateadmingroup", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ����һ���µĹ�������Ϣ
        /// </summary>
        /// <param name="__admingroupsInfo">Ҫ��ӵĹ�������Ϣ</param>
        /// <returns>���ļ�¼��</returns>
        public int CreateAdminGroupInfo(AdminGroupInfo adminGroupsInfo)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@admingid",(DbType)NpgsqlDbType.Smallint,2,adminGroupsInfo.Admingid),
									   DbHelper.MakeInParam("@alloweditpost",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Alloweditpost),
									   DbHelper.MakeInParam("@alloweditpoll",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Alloweditpoll),
									   DbHelper.MakeInParam("@allowstickthread",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowstickthread),
									   DbHelper.MakeInParam("@allowmodpost",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowmodpost),
									   DbHelper.MakeInParam("@allowdelpost",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowdelpost),
									   DbHelper.MakeInParam("@allowmassprune",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowmassprune),
									   DbHelper.MakeInParam("@allowrefund",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowrefund),
									   DbHelper.MakeInParam("@allowcensorword",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowcensorword),
									   DbHelper.MakeInParam("@allowviewip",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowviewip),
									   DbHelper.MakeInParam("@allowbanip",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowbanip),
									   DbHelper.MakeInParam("@allowedituser",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowedituser),
									   DbHelper.MakeInParam("@allowmoduser",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowmoduser),
									   DbHelper.MakeInParam("@allowbanuser",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowbanuser),
									   DbHelper.MakeInParam("@allowpostannounce",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowpostannounce),
									   DbHelper.MakeInParam("@allowviewlog",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowviewlog),
									   DbHelper.MakeInParam("@disablepostctrl",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Disablepostctrl),
                                       DbHelper.MakeInParam("@allowviewrealname",(DbType)NpgsqlDbType.Smallint,1,adminGroupsInfo.Allowviewrealname)
								   };
            return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}createadmingroup", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ɾ��ָ���Ĺ�������Ϣ
        /// </summary>
        /// <param name="admingid">������ID</param>
        /// <returns>���ļ�¼��</returns>
        public int DeleteAdminGroupInfo(short adminGid)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@admingid",(DbType)NpgsqlDbType.Smallint,2,adminGid),
								   };
            string commandText = string.Format("DELETE FROM [{0}admingroups] WHERE [admingid] = @admingid",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }


        public void UpdateRaterangeByGroupid(string rateRange, int groupId)
        {
            DbParameter[] parms = 
			{
                DbHelper.MakeInParam("@raterange",(DbType)NpgsqlDbType.Char, 500,rateRange),
				DbHelper.MakeInParam("@groupid",(DbType)NpgsqlDbType.Integer, 4,groupId)
			};
            string commandText = string.Format("UPDATE [{0}usergroups] SET [raterange]=@raterange WHERE [groupid]=@groupid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void ClearAuthstrByUidlist(string uidList)
        {
            string commandText = string.Format("UPDATE [{0}userfields] SET [authstr]='' WHERE [uid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                uidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public void DeleteUserByUidlist(string uidList)
        {
            string commandText = string.Format("DELETE FROM [{0}userfields] WHERE [uid] IN({1})",
                                                BaseConfigs.GetTablePrefix,
                                                uidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);

            commandText = string.Format("DELETE FROM [{0}users] WHERE [uid] IN({1})",
                                         BaseConfigs.GetTablePrefix,
                                         uidList);
            int deleteUserCount = DbHelper.ExecuteNonQuery(CommandType.Text, commandText);

            commandText = string.Format("UPDATE [{0}statistics] SET [totalusers]=[totalusers]-{1}",
                                         BaseConfigs.GetTablePrefix,
                                         deleteUserCount);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public DataTable GetUsersByUidlLst(string uidList)
        {
            string commandText = string.Format("SELECT {0} FROM [{1}users] WHERE [uid] IN ({2})",
                                                DbFields.USERS,
                                                BaseConfigs.GetTablePrefix,
                                                uidList);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetUserGroup()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}usergroups] WHERE [radminid]= 0 AND [groupid]>8 AND [system]=0 ORDER BY [groupid]",
                                                DbFields.USER_GROUPS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public string GetUserGroupTitle()
        {
            return string.Format("SELECT [groupid],[grouptitle] FROM [{0}usergroups] WHERE [radminid]= 0 And [groupid]>8 ORDER BY [groupid]",
                                  BaseConfigs.GetTablePrefix);
        }

        public DataTable GetUserGroupWithOutGuestTitle()
        {
            string commandText = string.Format("SELECT [groupid],[grouptitle] FROM [{0}usergroups] WHERE [groupid]<>7  ORDER BY [groupid] ASC",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }

        public void DeleteUserGroupInfo(int groupId)
        {
            string commandText = string.Format("DELETE FROM [{0}usergroups] Where [groupid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void ChangeUsergroup(int sourceUserGroupId, int targetUserGroupId)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@soureceusergroupid",(DbType)NpgsqlDbType.Integer, 4,sourceUserGroupId),
                DbHelper.MakeInParam("@targetusergroupid",(DbType)NpgsqlDbType.Integer, 4,targetUserGroupId)
			};
            string commandText = string.Format("UPDATE [{0}users] SET [groupid]=@targetusergroupid WHERE [groupid]=@soureceusergroupid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateUserAdminIdByGroupId(int adminId, int groupId)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@adminid",(DbType)NpgsqlDbType.Integer, 4,adminId),
                DbHelper.MakeInParam("@groupid",(DbType)NpgsqlDbType.Integer, 4,groupId)
			};
            string commandText = string.Format("UPDATE [{0}users] SET [adminid]=@adminid WHERE [groupid]=@groupid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public bool IsExistMedalAwardRecord(int medalId, int userId)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@medalid", (DbType)NpgsqlDbType.Integer,4, medalId),
				DbHelper.MakeInParam("@userid",(DbType)NpgsqlDbType.Integer,4,userId)
			};
            string commandText = string.Format("SELECT TOP 1 ID FROM [{0}medalslog] WHERE [medals]=@medalid AND [uid]=@userid",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0].Rows.Count != 0;
        }

        public void AddMedalslog(int adminId, string adminName, string ip, string userName, int uid, string actions, int medals, string reason)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@adminid", (DbType)NpgsqlDbType.Integer,4, adminId),
				DbHelper.MakeInParam("@adminname",(DbType)NpgsqlDbType.Varchar,50,adminName),
                DbHelper.MakeInParam("@ip", (DbType)NpgsqlDbType.Varchar,15, ip),
				DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Varchar,50,userName),
                DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer,4, uid),
				DbHelper.MakeInParam("@actions",(DbType)NpgsqlDbType.Varchar,100,actions),
                DbHelper.MakeInParam("@medals", (DbType)NpgsqlDbType.Integer,4, medals),
				DbHelper.MakeInParam("@reason",(DbType)NpgsqlDbType.Varchar,100,reason)
			};
            string commandText = string.Format("INSERT INTO [{0}medalslog] (adminid,adminname,ip,username,uid,actions,medals,reason) VALUES (@adminid,@adminname,@ip,@username,@uid,@actions,@medals,@reason)",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateMedalslog(string newActions, DateTime postDateTime, string reason, string oldActions, int medals, int uid)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@newactions",(DbType)NpgsqlDbType.Varchar,100,newActions),
                DbHelper.MakeInParam("@postdatetime",(DbType)NpgsqlDbType.Timestamp,8,postDateTime),
				DbHelper.MakeInParam("@reason",(DbType)NpgsqlDbType.Varchar,100,reason),
                DbHelper.MakeInParam("@oldactions",(DbType)NpgsqlDbType.Varchar,100,oldActions),
                DbHelper.MakeInParam("@medals", (DbType)NpgsqlDbType.Integer,4, medals),
                DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer,4, uid)
			};
            string commandText = string.Format("UPDATE [{0}medalslog] SET [actions]=@newactions ,[postdatetime]=@postdatetime, reason=@reason  WHERE [actions]=@oldactions AND [medals]=@medals  AND [uid]=@uid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateMedalslog(string actions, DateTime postDateTime, string reason, int uid)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@actions",(DbType)NpgsqlDbType.Varchar,100,actions),
                DbHelper.MakeInParam("@postdatetime",(DbType)NpgsqlDbType.Timestamp,8,postDateTime),
				DbHelper.MakeInParam("@reason",(DbType)NpgsqlDbType.Varchar,100,reason),
                DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer,4, uid)
			};
            string commandText = string.Format("Update [{0}medalslog] SET [actions]=@actions ,[postdatetime]=@postdatetime,[reason]=@reason  WHERE [uid]=@uid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void SetStopTalkUser(string uidList)
        {
            string commandText = string.Format("UPDATE [{0}users] SET [groupid]=4, [adminid]=0, [groupexpiry]=0 WHERE [uid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                uidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void ChangeUserGroupByUid(int groupId, string uidList)
        {
            string commandText = string.Format("UPDATE [{0}users] SET [groupid]={1}  WHERE [uid] IN ({2})",
                                                BaseConfigs.GetTablePrefix,
                                                groupId,
                                                uidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void DeletePostByPosterid(int tableId, int posterId)
        {
            string commandText = string.Format("DELETE FROM  [{0}posts{1}]  WHERE [posterid]={2}",
                                                BaseConfigs.GetTablePrefix,
                                                tableId,
                                                posterId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// ɾ��ָ���û�����
        /// </summary>
        /// <param name="uid"></param>
        public void DeleteAttachmentByUid(int uid, int days)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
                                      DbHelper.MakeInParam("@days", (DbType)NpgsqlDbType.Integer, 4, days)
                                  };
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}deleteAttachmentByUid", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ɾ���û�ָ�������ڵ�����
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="days">����</param>
        public void DeletePostByUidAndDays(int uid, int days)
        {
            DbParameter[] parms = { 
                                    DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid),
                                    DbHelper.MakeInParam("@days",(DbType)NpgsqlDbType.Integer,4,days)
                                  };
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}deletepostsbyuidanddays", BaseConfigs.GetTablePrefix), parms);
        }

        public void DeleteTopicByPosterid(int posterId)
        {
            string commandText = string.Format("DELETE FROM [{0}topics] WHERE [posterid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                posterId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void ClearPosts(int uid)
        {
            string commandText = string.Format("UPDATE [{0}users] SET [digestposts]=0 , [posts]=0  WHERE [uid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                uid);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void UpdateEmailValidateInfo(string authStr, DateTime authTime, int uid)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@authstr",(DbType)NpgsqlDbType.Varchar,20,authStr),
                DbHelper.MakeInParam("@authtime",(DbType)NpgsqlDbType.Timestamp,8,authTime),
                DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer,4, uid)
			};
            string commandText = string.Format("UPDATE [{0}userfields] SET [Authstr]=@authstr,[Authtime]=@authtime ,[Authflag]=1  WHERE [uid]=@uid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }


        public DataTable GetUserEmailByGroupid(string groupIdList)
        {
            string commandText = string.Format("SELECT [username],[Email]  From [{0}users] WHERE [Email] Is Not null AND [Email]<>'' AND [groupid] IN({1})",
                                                BaseConfigs.GetTablePrefix,
                                                groupIdList);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetUserGroupExceptGroupid(int groupId)
        {
            string commandText = string.Format("SELECT [groupid] FROM [{0}usergroups] WHERE [radminid]=0 And [groupid]>8 AND [groupid]<>{1}",
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        /// <summary>
        /// �����ղ���Ϣ
        /// </summary>
        /// <param name="uid">�û�ID</param>
        /// <param name="tid">����ID</param>
        /// <param name="type">�ղ����ͣ�0=���⣬1=��ᣬ2=������־</param>
        /// <returns>�����ɹ����� 1 ���򷵻� 0</returns>	
        public int CreateFavorites(int uid, int tid, byte type)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid),
									   DbHelper.MakeInParam("@tid",(DbType)NpgsqlDbType.Integer,4,tid),
                                       DbHelper.MakeInParam("@type", (DbType)NpgsqlDbType.Smallint, 4, type)
								   };
            return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}createfavorite", BaseConfigs.GetTablePrefix), parms);
        }



        /// <summary>
        /// ɾ��ָ���û����ղ���Ϣ
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="fitemid">Ҫɾ�����ղ���Ϣid�б�,��Ӣ�Ķ��ŷָ�</param>
        /// <returns>ɾ��������������ʱ���� -1</returns>
        public int DeleteFavorites(int uid, string fidList, byte type)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer,4, uid),
                                       DbHelper.MakeInParam("@typeid", (DbType)NpgsqlDbType.Smallint, 1, type)
			                        };
            string commandText = string.Format("DELETE FROM [{0}favorites] WHERE [tid] IN ({1}) AND [uid] = @uid AND [typeid]=@typeid",
                                                BaseConfigs.GetTablePrefix,
                                                fidList);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }


        /// <summary>
        /// �õ��û��ղ���Ϣ�б�
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="pagesize">��ҳʱÿҳ�ļ�¼��</param>
        /// <param name="pageindex">��ǰҳ��</param>
        /// <param name="typeid">�ղ�����id</param>
        /// <returns>�û���Ϣ�б�</returns>
        public DataTable GetFavoritesList(int uid, int pageSize, int pageIndex, int typeId)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid),
									   DbHelper.MakeInParam("@pagesize", (DbType)NpgsqlDbType.Integer,4,pageSize),
									   DbHelper.MakeInParam("@pageindex",(DbType)NpgsqlDbType.Integer,4,pageIndex)								   
								   };
            switch (typeId)
            {
                case 1:
                    return DbHelper.ExecuteDataset(CommandType.StoredProcedure,
                                                   string.Format("{0}getfavoriteslistbyalbum", BaseConfigs.GetTablePrefix),
                                                   parms).Tables[0];
                case 2:
                    return DbHelper.ExecuteDataset(CommandType.StoredProcedure,
                                                   string.Format("{0}getfavoriteslistbyspacepost", BaseConfigs.GetTablePrefix),
                                                   parms).Tables[0];
                case 3:
                    {   //��ȡ�ղص���Ʒ��Ϣ
                        string commandText = string.Format("SELECT [f].[tid], [f].[uid], [goodsid], [shopid], [categoryid] , [title] , [price], [selleruid], [seller], [dateline], [expiration]  FROM [{0}favorites] [f],[{0}goods] [goods] WHERE [f].[tid]=[goods].[goodsid] AND [f].[typeid]=3  AND [f].[uid]={1} ",
                                                            BaseConfigs.GetTablePrefix,
                                                            uid);
                        if (pageIndex == 1)
                            commandText = string.Format("SELECT TOP {0}  [tid], [uid], [goodsid], [shopid], [categoryid] , [title] , [price], [selleruid] AS [posterid], [seller] AS [poster], [dateline] AS [postdatetime], [expiration]  FROM ( {1} ) f  ORDER BY [tid] DESC",
                                                         pageSize,
                                                         commandText);
                        else
                            commandText = string.Format("SELECT TOP {0}  [tid], [uid], [goodsid], [shopid], [categoryid] , [title] , [price], [selleruid] AS [posterid], [seller] AS [poster], [dateline] AS [postdatetime], [expiration]  FROM ( {1} ) f1 WHERE [tid] < (SELECT MIN([tid]) FROM (SELECT TOP {2} [tid] FROM ({1}) f2  ORDER BY [tid] DESC) AS tblTmp) ORDER BY [tid] DESC",
                                                         pageSize,
                                                         commandText,
                                                         (pageIndex - 1) * pageSize);
                        return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
                    }
                default:
                    return DbHelper.ExecuteDataset(CommandType.StoredProcedure,
                                                   string.Format("{0}getfavoriteslist", BaseConfigs.GetTablePrefix),
                                                   parms).Tables[0];
            }
        }

        public int GetFavoritesCount(int uid, int typeId)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer, 4, uid),
                                       DbHelper.MakeInParam("@typeid",(DbType)NpgsqlDbType.Smallint, 2, typeId)
								   };
            return TypeConverter.ObjectToInt(
                         DbHelper.ExecuteScalar(CommandType.Text,
                                                string.Format(" SELECT COUNT(uid)  FROM [dnt_favorites] WHERE [uid]=@uid AND [typeid]=@typeid", BaseConfigs.GetTablePrefix),
                                                parms));
        }

        /// <summary>
        /// �ղؼ����Ƿ������ָ��������
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="tid">����id</param>
        /// <returns></returns>
        public int CheckFavoritesIsIN(int uid, int tid, byte type)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid),
									   DbHelper.MakeInParam("@tid",(DbType)NpgsqlDbType.Integer,4,tid),
                                        DbHelper.MakeInParam("@type", (DbType)NpgsqlDbType.Smallint, 1, type)
			};
            string commandText = string.Format("SELECT COUNT([tid]) AS [tidcount] FROM [{0}favorites] WHERE [tid]=@tid AND [uid]=@uid AND [typeid]=@type",
                                                BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText, parms));
        }

        /// <summary>
        /// �����û��ղ���Ŀ�Ĳ鿴ʱ��
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="tid">����id</param>
        /// <returns></returns>
        public int UpdateUserFavoriteViewTime(int uid, int tid)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid),
									   DbHelper.MakeInParam("@tid",(DbType)NpgsqlDbType.Integer,4,tid)
			};

            return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateuserfavoriteviewtime", BaseConfigs.GetTablePrefix), parms);
        }

        public void UpdateUserShortInfo(string location, string bio, string signature, int uid)
        {
            //����ǩ�� location���� bio���˽���
            DbParameter[] parms ={
                                        DbHelper.MakeInParam("@signature",(DbType)NpgsqlDbType.Varchar,500,signature),
                                        DbHelper.MakeInParam("@location",(DbType)NpgsqlDbType.Varchar,50,location),
                                        DbHelper.MakeInParam("@bio",(DbType)NpgsqlDbType.Varchar,50,bio),
                                        DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid)        
                                    };
            string commandText = string.Format("Update [{0}userfields] SET signature=@signature, location=@location,bio=@bio WHERE uid=@uid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void DeleteModerator(int uid)
        {
            string commandText = string.Format("DELETE FROM [{0}moderators] WHERE [uid]={1}", BaseConfigs.GetTablePrefix, uid);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public void UpdatePMSenderAndReceiver(int uid, string newUserName)
        {
            DbParameter[] parms =  { 
                                        DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
                                        DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Varchar, 20, newUserName)
                                    };
            string commandText = string.Format("UPDATE [{0}pms] SET [msgfrom]=@username WHERE [msgfromid]=@uid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);

            commandText = string.Format("UPDATE [{0}pms] SET [msgto]=@username  WHERE [msgtoid]=@uid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        //public DataTable GetModerators(string oldUserName)
        //{
        //    DbParameter[] parms = {
        //        DbHelper.MakeInParam("@oldusername", (DbType)NpgsqlDbType.Varchar, 20, RegEsc(oldUserName))
        //                          };
        //    string commandText = string.Format("SELECT [fid],[moderators] FROM  [{0}forumfields] WHERE [moderators] LIKE '% @oldusername %'",
        //                                        BaseConfigs.GetTablePrefix);
        //    return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        //}


        public void CombinationUser(string postTableName, UserInfo targetUserInfo, UserInfo srcUserInfo)
        {
            DbParameter[] parms = {
					                DbHelper.MakeInParam("@target_uid", (DbType)NpgsqlDbType.Integer, 4, targetUserInfo.Uid),
					                DbHelper.MakeInParam("@target_username", (DbType)NpgsqlDbType.Char, 20, targetUserInfo.Username.Trim()),
					                DbHelper.MakeInParam("@src_uid", (DbType)NpgsqlDbType.Integer, 4, srcUserInfo.Uid)
				                  };
            string commandText = string.Format("UPDATE  [{0}topics] SET [posterid]=@target_uid,[poster]=@target_username  WHERE [posterid]=@src_uid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);

            commandText = string.Format("UPDATE  [{0}users] SET [posts]={1} WHERE [uid]=@target_uid",
                                         BaseConfigs.GetTablePrefix, srcUserInfo.Posts + targetUserInfo.Posts);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);

            commandText = string.Format("UPDATE  [{0}] SET [posterid]=@target_uid,[poster]=@target_username  WHERE [posterid]=@src_uid", postTableName);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);

            commandText = string.Format("UPDATE  [{0}pms] SET [msgtoid]=@target_uid,[msgto]=@target_username  WHERE [msgtoid]=@src_uid",
                                         BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);

            commandText = string.Format("UPDATE  [{0}attachments] SET [uid]=@target_uid WHERE [uid]=@src_uid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// ͨ���û����õ�UID
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public int GetUidByUserName(string userName)
        {
            DbParameter[] parms = 
            {
				DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Char, 20, userName)
			};

            string commandText = string.Format("SELECT TOP 1 [uid] FROM [{0}users] WHERE [username]=@username", BaseConfigs.GetTablePrefix);
            DataTable dt = DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
            return dt.Rows.Count > 0 ? TypeConverter.ObjectToInt(dt.Rows[0][0]) : 0;
        }

        /// <summary>
        /// ɾ��ָ���û���������Ϣ
        /// </summary>
        /// <param name="uid">ָ�����û�uid</param>
        /// <param name="delposts">�Ƿ�ɾ������</param>
        /// <param name="delpms">�Ƿ�ɾ������Ϣ</param>
        /// <returns></returns>
        public bool DelUserAllInf(int uid, bool delPosts, bool delPms)
        {
            Npgsql.NpgsqlConnection conn = new NpgsqlConnection(DbHelper.ConnectionString);
            conn.Open();
            using (NpgsqlTransaction trans = conn.BeginTransaction())
            {
                try
                {
                    DbParameter[] parms = {
                                            DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid),
						                    DbHelper.MakeInParam("@delPosts",(DbType)NpgsqlDbType.Bit,1, delPosts ? 1 : 0),
						                    DbHelper.MakeInParam("@delPms",(DbType)NpgsqlDbType.Bit,1,delPms ? 1 : 0)
                                          };
                    DbHelper.ExecuteNonQuery(trans, CommandType.StoredProcedure, string.Format("{0}deluserallinf", BaseConfigs.GetTablePrefix), parms);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
            conn.Close();
            return true;
        }

        public DataTable GetUserGroup(int groupId)
        {
            string commandText = string.Format("SELECT {0} FROM [{1}usergroups] WHERE [groupid]={2}",
                                                DbFields.USER_GROUPS,
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        public void AddUserGroup(UserGroupInfo userGroupInfo)
        {
            DbParameter[] parms = 
					{
						DbHelper.MakeInParam("@Radminid",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Radminid),
						DbHelper.MakeInParam("@Grouptitle",(DbType)NpgsqlDbType.Varchar,50, Utils.RemoveFontTag(userGroupInfo.Grouptitle)),
						DbHelper.MakeInParam("@Creditshigher",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Creditshigher),
						DbHelper.MakeInParam("@Creditslower",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Creditslower),
						DbHelper.MakeInParam("@Stars",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Stars),
						DbHelper.MakeInParam("@Color",(DbType)NpgsqlDbType.Char,7,userGroupInfo.Color),
						DbHelper.MakeInParam("@Groupavatar",(DbType)NpgsqlDbType.Varchar,60,userGroupInfo.Groupavatar),
						DbHelper.MakeInParam("@Readaccess",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Readaccess),
						DbHelper.MakeInParam("@Allowvisit",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowvisit),
						DbHelper.MakeInParam("@Allowpost",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowpost),
						DbHelper.MakeInParam("@Allowreply",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowreply),
						DbHelper.MakeInParam("@Allowpostpoll",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowpostpoll),
						DbHelper.MakeInParam("@Allowdirectpost",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowdirectpost),
						DbHelper.MakeInParam("@Allowgetattach",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowgetattach),
						DbHelper.MakeInParam("@Allowpostattach",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowpostattach),
						DbHelper.MakeInParam("@Allowvote",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowvote),
						DbHelper.MakeInParam("@Allowmultigroups",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowmultigroups),
						DbHelper.MakeInParam("@Allowsearch",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsearch),
						DbHelper.MakeInParam("@Allowavatar",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowavatar),
						DbHelper.MakeInParam("@Allowcstatus",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowcstatus),
						DbHelper.MakeInParam("@Allowuseblog",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowuseblog),
						DbHelper.MakeInParam("@Allowinvisible",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowinvisible),
						DbHelper.MakeInParam("@Allowtransfer",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowtransfer),
						DbHelper.MakeInParam("@Allowsetreadperm",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsetreadperm),
						DbHelper.MakeInParam("@Allowsetattachperm",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsetattachperm),
						DbHelper.MakeInParam("@Allowhidecode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowhidecode),
						DbHelper.MakeInParam("@Allowhtml",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowhtml),
                        DbHelper.MakeInParam("@Allowhtmltitle",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowhtmltitle),
						DbHelper.MakeInParam("@Allowcusbbcode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowcusbbcode),
						DbHelper.MakeInParam("@Allownickname",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allownickname),
						DbHelper.MakeInParam("@Allowsigbbcode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsigbbcode),
						DbHelper.MakeInParam("@Allowsigimgcode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsigimgcode),
						DbHelper.MakeInParam("@Allowviewpro",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowviewpro),
						DbHelper.MakeInParam("@Allowviewstats",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowviewstats),
                        DbHelper.MakeInParam("@Allowtrade",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowtrade),
                        DbHelper.MakeInParam("@Allowdiggs",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowdiggs),
                        DbHelper.MakeInParam("@Allowdebate",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowdebate),
                        DbHelper.MakeInParam("@Allowbonus",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowbonus),
                        DbHelper.MakeInParam("@Minbonusprice",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Minbonusprice),
                        DbHelper.MakeInParam("@Maxbonusprice",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxbonusprice),
						DbHelper.MakeInParam("@Disableperiodctrl",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Disableperiodctrl),
						DbHelper.MakeInParam("@Reasonpm",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Reasonpm),
						DbHelper.MakeInParam("@Maxprice",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.Maxprice),
						DbHelper.MakeInParam("@Maxpmnum",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.Maxpmnum),
						DbHelper.MakeInParam("@Maxsigsize",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.Maxsigsize),
						DbHelper.MakeInParam("@Maxattachsize",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxattachsize),
						DbHelper.MakeInParam("@Maxsizeperday",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxsizeperday),
						DbHelper.MakeInParam("@Attachextensions",(DbType)NpgsqlDbType.Char,100,userGroupInfo.Attachextensions),
                        DbHelper.MakeInParam("@Maxspaceattachsize",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxspaceattachsize),
                        DbHelper.MakeInParam("@Maxspacephotosize",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxspacephotosize),
						DbHelper.MakeInParam("@Raterange",(DbType)NpgsqlDbType.Char,100,userGroupInfo.Raterange),
                        //DbHelper.MakeInParam("@Maxfriendscount",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.MaxFriendsCount),
                        DbHelper.MakeInParam("@ModNewTopics",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.ModNewTopics),
                        DbHelper.MakeInParam("@ModNewPosts",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.ModNewPosts),
                        DbHelper.MakeInParam("@Ignoreseccode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Ignoreseccode)
					};

            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}addusergroup", BaseConfigs.GetTablePrefix), parms);
        }

        public void AddOnlineList(string groupTitle)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@groupid", (DbType)NpgsqlDbType.Integer, 4, GetMaxUserGroupId()),
                                        DbHelper.MakeInParam("@title", (DbType)NpgsqlDbType.Varchar, 50, groupTitle)
                                    };
            string commandText = string.Format("INSERT INTO [{0}onlinelist] ([groupid], [title], [img]) VALUES(@groupid,@title, '')",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public DataTable GetMinCreditHigher()
        {
            string commandText = string.Format("SELECT MIN(Creditshigher) FROM [{0}usergroups] WHERE [groupid]>8 AND [radminid]=0 ",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetMaxCreditLower()
        {
            string commandText = string.Format("SELECT MAX(Creditslower) FROM [{0}usergroups] WHERE [groupid]>8 AND [radminid]=0 ",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetUserGroupByCreditshigher(int creditsHigher)
        {
            DbParameter parm = DbHelper.MakeInParam("@Creditshigher", (DbType)NpgsqlDbType.Integer, 4, creditsHigher);
            string commandText = string.Format("SELECT TOP 1 [groupid],[creditshigher],[creditslower] FROM [{0}usergroups] WHERE [groupid]>8 AND [radminid]=0  AND [Creditshigher]<=@Creditshigher AND @Creditshigher<[Creditslower]",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parm).Tables[0];
        }


        public void UpdateUserGroupCreidtsLower(int currentCreditsHigher, int creditsHigher)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@creditslower", (DbType)NpgsqlDbType.Integer, 4, creditsHigher),
                                        DbHelper.MakeInParam("@creditshigher", (DbType)NpgsqlDbType.Integer, 4, currentCreditsHigher)
                                    };
            string commandText = string.Format("UPDATE [{0}usergroups] SET [creditslower]=@creditslower WHERE [groupid]>8 AND [radminid]=0 AND [creditshigher]=@creditshigher",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public DataTable GetUserGroupByCreditsHigherAndLower(int creditsHigher, int creditsLower)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@Creditshigher", (DbType)NpgsqlDbType.Integer, 4, creditsHigher),
                                        DbHelper.MakeInParam("@Creditslower", (DbType)NpgsqlDbType.Integer, 4, creditsLower)
                                    };
            string commandText = string.Format("SELECT [groupid] FROM [{0}usergroups] WHERE [groupid]>8 AND [radminid]=0 AND [Creditshigher]=@Creditshigher AND [Creditslower]=@Creditslower",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        }

        public int GetGroupCountByCreditsLower(int creditsHigher)
        {
            string commandText = string.Format("SELECT [groupid] FROM [{0}usergroups] WHERE [groupid]>8 AND [radminid]=0 AND [creditslower]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                creditsHigher);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0].Rows.Count;
        }

        public void UpdateUserGroupsCreditsLowerByCreditsLower(int creditsLower, int creditsHigher)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@Creditshigher", (DbType)NpgsqlDbType.Integer, 4, creditsHigher),
                                        DbHelper.MakeInParam("@Creditslower", (DbType)NpgsqlDbType.Integer, 4, creditsLower)
                                    };
            string commandText = string.Format("UPDATE [{0}usergroups] SET [creditslower]=@Creditslower WHERE [groupid]>8 AND [radminid]=0 AND [creditslower]=@Creditshigher",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteDataset(CommandType.Text, commandText, parms);
        }


        public void UpdateUserGroupsCreditsHigherByCreditsHigher(int creditsHigher, int creditsLower)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@Creditshigher", (DbType)NpgsqlDbType.Integer, 4, creditsHigher),
                                        DbHelper.MakeInParam("@Creditslower", (DbType)NpgsqlDbType.Integer, 4, creditsLower)
            };
            string commandText = string.Format("UPDATE [{0}usergroups] SET [Creditshigher]=@Creditshigher WHERE [groupid]>8 AND [radminid]=0 AND [Creditshigher]=@Creditslower",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteDataset(CommandType.Text, commandText, parms);
        }

        public DataTable GetUserGroupCreditsLowerAndHigher(int groupiId)
        {
            string commandText = string.Format("SELECT TOP 1 [groupid],[creditshigher],[creditslower] FROM [{0}usergroups]  WHERE [groupid]= {1}",
                                                BaseConfigs.GetTablePrefix,
                                                groupiId);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public void UpdateUserGroup(UserGroupInfo userGroupInfo)
        {
            DbParameter[] parms = 
					{
						DbHelper.MakeInParam("@Radminid",(DbType)NpgsqlDbType.Integer,4,(userGroupInfo.Groupid == 1) ? 1 : userGroupInfo.Radminid),
						DbHelper.MakeInParam("@Grouptitle",(DbType)NpgsqlDbType.Varchar,50, Utils.RemoveFontTag(userGroupInfo.Grouptitle)),
						DbHelper.MakeInParam("@Creditshigher",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Creditshigher),
						DbHelper.MakeInParam("@Creditslower",(DbType)NpgsqlDbType.Integer,4, userGroupInfo.Creditslower),
						DbHelper.MakeInParam("@Stars",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Stars),
						DbHelper.MakeInParam("@Color",(DbType)NpgsqlDbType.Char,7,userGroupInfo.Color),
						DbHelper.MakeInParam("@Groupavatar",(DbType)NpgsqlDbType.Varchar,60,userGroupInfo.Groupavatar),
						DbHelper.MakeInParam("@Readaccess",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Readaccess),
						DbHelper.MakeInParam("@Allowvisit",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowvisit),
						DbHelper.MakeInParam("@Allowpost",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowpost),
						DbHelper.MakeInParam("@Allowreply",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowreply),
						DbHelper.MakeInParam("@Allowpostpoll",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowpostpoll),
						DbHelper.MakeInParam("@Allowdirectpost",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowdirectpost),
						DbHelper.MakeInParam("@Allowgetattach",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowgetattach),
						DbHelper.MakeInParam("@Allowpostattach",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowpostattach),
						DbHelper.MakeInParam("@Allowvote",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowvote),
						DbHelper.MakeInParam("@Allowmultigroups",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowmultigroups),
						DbHelper.MakeInParam("@Allowsearch",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsearch),
						DbHelper.MakeInParam("@Allowavatar",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowavatar),
						DbHelper.MakeInParam("@Allowcstatus",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowcstatus),
						DbHelper.MakeInParam("@Allowuseblog",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowuseblog),
						DbHelper.MakeInParam("@Allowinvisible",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowinvisible),
						DbHelper.MakeInParam("@Allowtransfer",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowtransfer),
						DbHelper.MakeInParam("@Allowsetreadperm",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsetreadperm),
						DbHelper.MakeInParam("@Allowsetattachperm",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsetattachperm),
						DbHelper.MakeInParam("@Allowhidecode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowhidecode),
						DbHelper.MakeInParam("@Allowhtml",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowhtml),
                        DbHelper.MakeInParam("@Allowhtmltitle",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowhtmltitle),
						DbHelper.MakeInParam("@Allowcusbbcode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowcusbbcode),
						DbHelper.MakeInParam("@Allownickname",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allownickname),
						DbHelper.MakeInParam("@Allowsigbbcode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsigbbcode),
						DbHelper.MakeInParam("@Allowsigimgcode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowsigimgcode),
						DbHelper.MakeInParam("@Allowviewpro",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowviewpro),
						DbHelper.MakeInParam("@Allowviewstats",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowviewstats),
                        DbHelper.MakeInParam("@Allowtrade",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowtrade),
                        DbHelper.MakeInParam("@Allowdiggs",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowdiggs),
						DbHelper.MakeInParam("@Disableperiodctrl",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Disableperiodctrl),
                        DbHelper.MakeInParam("@Allowdebate",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowdebate),
                        DbHelper.MakeInParam("@Allowbonus",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Allowbonus),
                        DbHelper.MakeInParam("@Minbonusprice",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Minbonusprice),
                        DbHelper.MakeInParam("@Maxbonusprice",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxbonusprice),
						DbHelper.MakeInParam("@Reasonpm",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Reasonpm),
						DbHelper.MakeInParam("@Maxprice",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.Maxprice),
						DbHelper.MakeInParam("@Maxpmnum",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.Maxpmnum),
						DbHelper.MakeInParam("@Maxsigsize",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.Maxsigsize),
						DbHelper.MakeInParam("@Maxattachsize",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxattachsize),
						DbHelper.MakeInParam("@Maxsizeperday",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxsizeperday),
						DbHelper.MakeInParam("@Attachextensions",(DbType)NpgsqlDbType.Char,100,userGroupInfo.Attachextensions),
                        DbHelper.MakeInParam("@Maxspaceattachsize",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxspaceattachsize),
                        DbHelper.MakeInParam("@Maxspacephotosize",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Maxspacephotosize),
						DbHelper.MakeInParam("@Groupid",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Groupid),
                        //DbHelper.MakeInParam("@Maxfriendscount",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.MaxFriendsCount),
                        DbHelper.MakeInParam("@ModNewTopics",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.ModNewTopics),
                        DbHelper.MakeInParam("@ModNewPosts",(DbType)NpgsqlDbType.Smallint,2,userGroupInfo.ModNewPosts),
                        DbHelper.MakeInParam("@Ignoreseccode",(DbType)NpgsqlDbType.Integer,4,userGroupInfo.Ignoreseccode)
			};
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateusergroup", BaseConfigs.GetTablePrefix), parms);
        }


        public void UpdateOnlineList(UserGroupInfo userGroupInfo)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@groupid", (DbType)NpgsqlDbType.Integer, 4, userGroupInfo.Groupid),
                                        DbHelper.MakeInParam("@title", (DbType)NpgsqlDbType.Varchar, 50, Utils.RemoveFontTag(userGroupInfo.Grouptitle))
                                    };
            DbHelper.ExecuteNonQueryInMasterDB(CommandType.StoredProcedure, string.Format("{0}updateonlinelist", BaseConfigs.GetTablePrefix), parms);
        }

        public bool IsSystemOrTemplateUserGroup(int groupId)
        {
            string commandText = string.Format("SELECT TOP 1 {0}  FROM [{1}usergroups] WHERE ([system]=1 OR [type]=1) AND [groupid]={2}",
                                                DbFields.USER_GROUPS,
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0].Rows.Count > 0;
        }

        public string GetUserGroupRAdminId(int groupId)
        {
            string commandText = string.Format("SELECT TOP 1 [radminid] FROM [{0}usergroups] WHERE  [groupid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0].Rows[0][0].ToString();
        }

        public void UpdateUserGroupLowerAndHigherToLimit(int groupId)
        {
            string commandText = string.Format("UPDATE [{0}usergroups] SET [creditshigher]=-9999999 ,creditslower=9999999  WHERE [groupid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public void DeleteOnlineList(int groupId)
        {
            string commandText = string.Format("DELETE FROM [{0}onlinelist] WHERE [groupid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public int GetMaxUserGroupId()
        {
            string commandText = string.Format("SELECT COALESCE(MAX(groupid), 0) FROM [{0}usergroups]", BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText));
        }


        public bool DeletePaymentLog()
        {
            try
            {
                DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM [{0}paymentlog]", BaseConfigs.GetTablePrefix));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ��ָ������ɾ����־
        /// </summary>
        /// <param name="condition">��ѯ����</param>
        /// <returns></returns>
        public bool DeletePaymentLog(string condition)
        {
            try
            {
                DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM [{0}paymentlog] WHERE {1}", BaseConfigs.GetTablePrefix, condition));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DataTable GetPaymentLogList(int pageSize, int currentPage)
        {
            int pagetop = (currentPage - 1) * pageSize;
            string commandText;
            if (currentPage == 1)
                commandText = string.Format("SELECT  {1}, {2}topics.fid AS fid ,{2}topics.postdatetime AS postdatetime ,{2}topics.poster AS authorname, {2}topics.title AS title,{2}users.username As UserName  FROM [{2}paymentlog] AS [pl] LEFT OUTER JOIN {2}topics ON pl.tid = {2}topics.tid LEFT OUTER JOIN {2}users ON {2}users.uid = pl.uid ORDER BY [pl].[id] DESC  LIMIT {0}",
                                             pageSize,
                                             DbFields.PAYMENT_LOG_JOIN,
                                             BaseConfigs.GetTablePrefix);
            else
                commandText = string.Format("SELECT TOP {0} {1}, {2}topics.fid AS fid ,{2}topics.postdatetime AS postdatetime ,{2}topics.poster AS authorname, {2}topics.title AS title,{2}users.username As UserName  FROM [{2}paymentlog] AS [pl] LEFT OUTER JOIN {2}topics ON pl.tid = {2}topics.tid LEFT OUTER JOIN {2}users ON {2}users.uid = pl.uid WHERE [id] < (SELECT min([id])  FROM (SELECT TOP {3} [id] FROM [{2}paymentlog] ORDER BY [id] DESC) AS tblTmp )  ORDER BY {2}[pl].[id] DESC",
                                             pageSize,
                                             DbFields.PAYMENT_LOG_JOIN,
                                             BaseConfigs.GetTablePrefix,
                                             pagetop);

            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetPaymentLogList(int pageSize, int currentPage, string condition)
        {
            int pagetop = (currentPage - 1) * pageSize;
            string commandText;
            if (currentPage == 1)
                commandText = string.Format("SELECT TOP {0} {1}, {2}topics.fid AS fid ,{2}topics.postdatetime AS postdatetime ,{2}topics.poster AS authorname, {2}topics.title AS title,{2}users.username As UserName  FROM [{2}paymentlog] AS [pl] LEFT OUTER JOIN {2}topics ON pl.tid = {2}topics.tid LEFT OUTER JOIN {2}users ON {2}users.uid = pl.uid WHERE {3}  Order by [pl].[id] DESC",
                                             pageSize,
                                             DbFields.PAYMENT_LOG_JOIN,
                                             BaseConfigs.GetTablePrefix,
                                             condition);
            else
                commandText = string.Format("SELECT TOP {0} {1}, {2}topics.fid AS fid ,{2}topics.postdatetime AS postdatetime ,{2}topics.poster AS authorname, {2}topics.title AS title,{2}users.username As UserName  FROM [{2}paymentlog] AS [pl] LEFT OUTER JOIN {2}topics ON pl.tid = {2}topics.tid LEFT OUTER JOIN {2}users ON {2}users.uid = pl.uid  WHERE [id] < (SELECT min([id])  FROM (SELECT TOP {3} [id] FROM [{2}paymentlog] WHERE {4} ORDER BY [id] DESC) AS tblTmp ) AND {4} ORDER BY [pl].[id] DESC",
                                             pageSize,
                                             DbFields.PAYMENT_LOG_JOIN,
                                             BaseConfigs.GetTablePrefix,
                                             pagetop,
                                             condition);

            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        /// <summary>
        /// �õ����ֽ�����־��¼��
        /// </summary>
        /// <returns></returns>
        public int GetPaymentLogListCount()
        {
            string commandText = string.Format("SELECT count(id) FROM [{0}paymentlog]", BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0].Rows[0][0]);
        }

        /// <summary>
        /// �õ�ָ����ѯ�����µĻ��ֽ�����־��
        /// </summary>
        /// <param name="condition">��ѯ����</param>
        /// <returns></returns>
        public int GetPaymentLogListCount(string condition)
        {
            string commandText = string.Format("SELECT count(pl.id) FROM [{0}paymentlog] AS [pl] WHERE {1}",
                                                BaseConfigs.GetTablePrefix,
                                                condition);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0].Rows[0][0]);
        }

        public void DeleteModeratorByFid(int fid)
        {
            string commandText = string.Format("DELETE FROM [{0}moderators] WHERE [fid]={1}", BaseConfigs.GetTablePrefix, fid);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public DataTable GetUidModeratorByFid(string fidList)
        {
            string commandText = string.Format("SELECT distinct [uid] FROM [{0}moderators] WHERE [fid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                fidList);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public void AddModerator(int uid, int fid, int displayOrder, int inherited)
        {
            DbParameter[] parms = 
            {
                DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
                DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Smallint, 2, fid),
                DbHelper.MakeInParam("@displayorder", (DbType)NpgsqlDbType.Smallint, 2, displayOrder),
                DbHelper.MakeInParam("@inherited", (DbType)NpgsqlDbType.Smallint, 2, inherited)
		    };
            string commandText = string.Format("INSERT INTO [{0}moderators] (uid,fid,displayorder,inherited) VALUES(@uid,@fid,@displayorder,@inherited)",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public DataTable GetModeratorInfo(string moderator)
        {
            DbParameter[] parms = {
				                      DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Varchar, 20, moderator.Trim())
			                      };
            string commandText = string.Format("SELECT TOP 1 [uid],[groupid]  FROM [{0}users] Where [groupid]<>7 AND [groupid]<>8 AND [username]=@username",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        }

        public void SetModerator(string moderator)
        {
            DbParameter[] parms = 
            {
				DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Varchar, 20, moderator.Trim())
			};
            string commandText = string.Format("UPDATE [{0}users] SET [adminid]=3,[groupid]=3 WHERE [username]=@username",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);

            commandText = string.Format("UPDATE [{0}online] SET [adminid]=3,[groupid]=3 WHERE [username]=@username",
                                         BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }


        public DataTable GetUidAdminIdByUsername(string userName)
        {
            DbParameter[] parms =
			{
				DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Varchar, 20, userName)
			};
            string commandText = string.Format("SELECT   [uid],[adminid] FROM [{0}users] WHERE [username] = @username  limit 1",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        }

        public DataTable GetUidInModeratorsByUid(int currentFid, int uid)
        {
            DbParameter[] parms =
			{
				DbHelper.MakeInParam("@currentfid", (DbType)NpgsqlDbType.Integer, 4, currentFid),
                DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid)
			};
            string commandText = string.Format("SELECT   [uid]  FROM [{0}moderators] WHERE [fid]<>@currentfid AND [uid]=@uid  limit 1",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        }

        public void UpdateUserOnlineInfo(int groupId, int userId)
        {
            DbParameter[] parms =
			{
				DbHelper.MakeInParam("@groupid", (DbType)NpgsqlDbType.Integer, 4, groupId),
                DbHelper.MakeInParam("@userid", (DbType)NpgsqlDbType.Integer, 4, userId)
			};
            string commandText = string.Format("UPDATE [{0}online] SET [groupid]=@groupid WHERE [userid]=@userid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateUserOtherInfo(int groupId, int userId)
        {
            DbParameter[] parms =
			{
				DbHelper.MakeInParam("@groupid", (DbType)NpgsqlDbType.Integer, 4, groupId),
                DbHelper.MakeInParam("@userid", (DbType)NpgsqlDbType.Integer, 4, userId)
			};
            string commandText = string.Format("UPDATE [{0}users] SET [groupid]=@groupid ,[adminid]=0 WHERE [uid]=@userid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// �õ���̳�����ע����û�ID���û���
        /// </summary>
        /// <param name="lastuserid">������������ע����û�ID</param>
        /// <param name="lastusername">������������ע����û���</param>
        /// <returns>���ڷ���true,�����ڷ���false</returns>
        public bool GetLastUserInfo(out string lastUserId, out string lastUserName)
        {
            lastUserId = "";
            lastUserName = "";
            string commandText = string.Format("SELECT TOP 1 [uid],[username] FROM [{0}users] ORDER BY [uid] DESC", BaseConfigs.GetTablePrefix);
            IDataReader reader = DbHelper.ExecuteReader(CommandType.Text, commandText);
            if (reader.Read())
            {
                lastUserId = reader["uid"].ToString();
                lastUserName = reader["username"].ToString().Trim();
                reader.Close();
                return true;
            }
            reader.Close();
            return false;
        }

        public IDataReader GetTopUsers(int statCount, int lastUid)
        {
            DbParameter[] parms =
			{
				DbHelper.MakeInParam("@statcont", (DbType)NpgsqlDbType.Integer, 4, statCount),
                DbHelper.MakeInParam("@lastuid", (DbType)NpgsqlDbType.Integer, 4, lastUid)
			};
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("{0}gettopusers", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �ؽ������û��ľ�������
        /// </summary>
        public void ResetUserDigestPosts()
        {
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}resetuserdigestposts", BaseConfigs.GetTablePrefix));
        }

        public IDataReader GetUsers(int startUid, int endUid)
        {
            DbParameter[] parms = {
				DbHelper.MakeInParam("@start_uid", (DbType)NpgsqlDbType.Integer, 4, startUid),
				DbHelper.MakeInParam("@end_uid", (DbType)NpgsqlDbType.Integer, 4, endUid)
			};
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("{0}getusers", BaseConfigs.GetTablePrefix), parms);
        }

        public void UpdateUserPostCount(int postCount, int userId)
        {
            DbParameter[] parms = {
                                        DbHelper.MakeInParam("@postcount", (DbType)NpgsqlDbType.Integer, 4, postCount),
                                        DbHelper.MakeInParam("@userid", (DbType)NpgsqlDbType.Integer, 4, userId)
                                    };
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateuserpostcount", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ���������û���������
        /// </summary>
        /// <param name="postTableID"></param>
        public void UpdateAllUserPostCount(int postTableID)
        {
            if (postTableID == 1)
            {
                DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE [{0}users] SET posts = 0 WHERE posts > 0", BaseConfigs.GetTablePrefix));
            }
            DbParameter parm = DbHelper.MakeInParam("@posttablename", (DbType)NpgsqlDbType.Varchar, 50, string.Format("{0}posts{1}", BaseConfigs.GetTablePrefix, postTableID));
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}resetuserspostcount", BaseConfigs.GetTablePrefix), parm);
        }

        /// <summary>
        /// �����ҵ�����
        /// </summary>
        /// <param name="postTableID"></param>
        public void UpdateMyPost(int postTableID)
        {
            DbParameter parm = DbHelper.MakeInParam("@tablename", (DbType)NpgsqlDbType.Varchar, 50, string.Format("{0}posts{1}", BaseConfigs.GetTablePrefix, postTableID));
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updatemypost", BaseConfigs.GetTablePrefix), parm);
        }
        /// <summary>
        /// ������а����б�
        /// </summary>
        /// <returns></returns>
        public DataTable GetModeratorList()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}moderators]", DbFields.MODERATORS, BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDatasetInMasterDB(CommandType.Text, commandText).Tables[0];
        }


        /// <summary>
        /// ���ȫ�������û���
        /// </summary>
        /// <returns></returns>
        public int GetOnlineAllUserCount()
        {
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalarInMasterDB(CommandType.Text,
                                                                    string.Format("SELECT COUNT(olid) FROM [{0}online]", BaseConfigs.GetTablePrefix)), 1);
        }

        /// <summary>
        /// �������߱�
        /// </summary>
        /// <returns></returns>
        public int CreateOnlineTable()
        {
            try
            {
                //��ʹ�ö�д���뷽��ʱ���޷�ʹ��TRUNCATE TABLE (���ݱ�ָ�),Ҫ������sql��������³�ʼ�����߱�
                if (DbSnapConfigs.GetConfig() != null && DbSnapConfigs.GetConfig().AppDbSnap)
                    return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM [{0}online];DBCC CHECKIDENT([{0}online],RESEED,0)", BaseConfigs.GetTablePrefix));
                else
                    return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("TRUNCATE TABLE [{0}online]", BaseConfigs.GetTablePrefix));
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// �������ע���û�������
        /// </summary>
        /// <returns>�û�����</returns>
        public int GetOnlineUserCount()
        {
            return TypeConverter.ObjectToInt(
                                 DbHelper.ExecuteDataset(CommandType.StoredProcedure, string.Format("{0}getonlineregistercount", BaseConfigs.GetTablePrefix)).Tables[0].Rows[0][0]);
        }

        /// <summary>
        /// ���ȫ�������û��б�
        /// </summary>
        /// <returns></returns>
        public DataTable GetOnlineUserListTable()
        {
            return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT [olid]      ,[userid]      ,[ip]      ,[username]      ,[nickname]      ,[password]      ,[groupid]      ,[olimg]      ,[adminid]      ,[invisible]      ,[action]      ,[lastactivity]      ,[lastposttime]      ,[lastpostpmtime]      ,[lastsearchtime]      ,[lastupdatetime]      ,[forumid]      ,[forumname]      ,[titleid]      ,[title]      ,[verifycode]      ,[newpms]      ,[newnotices] FROM [{0}online]", BaseConfigs.GetTablePrefix)).Tables[0];
        }

        /// <summary>
        /// ��ð�������û��б�
        /// </summary>
        /// <param name="forumid">���Id</param>
        /// <returns></returns>
        public IDataReader GetForumOnlineUserList(int forumId)
        {
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("{0}getonlineuserlistbyfid", BaseConfigs.GetTablePrefix),
                                          DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, forumId));
        }

        /// <summary>
        /// ���ȫ�������û��б�
        /// </summary>
        /// <returns></returns>
        public IDataReader GetOnlineUserList()
        {
            return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT [olid]      ,[userid]      ,[ip]      ,[username]      ,[nickname]      ,[password]      ,[groupid]      ,[olimg]      ,[adminid]      ,[invisible]      ,[action]      ,[lastactivity]      ,[lastposttime]      ,[lastpostpmtime]      ,[lastsearchtime]      ,[lastupdatetime]      ,[forumid]      ,[forumname]      ,[titleid]      ,[title]      ,[verifycode]      ,[newpms]      ,[newnotices] FROM [{0}online]", BaseConfigs.GetTablePrefix));
        }

        /// <summary>
        /// ���������û�ͼ��
        /// </summary>
        /// <returns></returns>
        public DataTable GetOnlineGroupIconTable()
        {
            return DbHelper.ExecuteDatasetInMasterDB(CommandType.Text, string.Format("SELECT [groupid], [displayorder], [title], [img] FROM [{0}onlinelist] WHERE [img] <> '' ORDER BY [displayorder]", BaseConfigs.GetTablePrefix)).Tables[0];
        }

        /// <summary>
        /// ����uid���olid
        /// </summary>
        /// <param name="uid">uid</param>
        /// <returns>olid</returns>
        public int GetOlidByUid(int uid)
        {
            string commandText = string.Format("SELECT [olid] FROM [{0}online] WHERE [userid]={1}", BaseConfigs.GetTablePrefix, uid);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalarToStr(CommandType.Text, commandText), -1);
        }

        /// <summary>
        /// ���ָ�������û�
        /// </summary>
        /// <param name="olid">����id</param>
        /// <returns>�����û�����ϸ��Ϣ</returns>
        public IDataReader GetOnlineUser(int olId)
        {
            string commandText = string.Format("SELECT {0} FROM [{1}online] WHERE [olid]={2}",
                                                DbFields.ONLINE,
                                                BaseConfigs.GetTablePrefix,
                                                olId);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        /// <summary>
        /// ���ָ���û�����ϸ��Ϣ
        /// </summary>
        /// <param name="userId">�����û�ID</param>
        /// <param name="password">�û�����</param>
        /// <returns>�û�����ϸ��Ϣ</returns>
        public DataTable GetOnlineUser(int userId, string passWord)
        {
            DbParameter[] parms =  { 
                                        DbHelper.MakeInParam("@userid", (DbType)NpgsqlDbType.Integer, 4, userId),
                                        DbHelper.MakeInParam("@password", (DbType)NpgsqlDbType.Char, 32, passWord)
                                    };
            return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT  [olid]      ,[userid]      ,[ip]      ,[username]      ,[nickname]      ,[password]      ,[groupid]      ,[olimg]      ,[adminid]      ,[invisible]      ,[action]      ,[lastactivity]      ,[lastposttime]      ,[lastpostpmtime]      ,[lastsearchtime]      ,[lastupdatetime]      ,[forumid]      ,[forumname]      ,[titleid]      ,[title]      ,[verifycode]      ,[newpms]      ,[newnotices]       FROM [{0}online] WHERE [userid]=@userid AND [password]=@password   LIMIT 1", BaseConfigs.GetTablePrefix), parms).Tables[0];
        }

        /// <summary>
        /// ���ָ���û�����ϸ��Ϣ
        /// </summary>
        /// <param name="userId">�����û�ID</param>
        /// <param name="ip">IP</param>
        /// <returns></returns>
        public DataTable GetOnlineUserByIP(int userId, string ip)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@userid", (DbType)NpgsqlDbType.Integer, 4, userId),
                                        DbHelper.MakeInParam("@ip", (DbType)NpgsqlDbType.Varchar, 15, ip)
                                    };
            return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT   [olid]      ,[userid]      ,[ip]      ,[username]      ,[nickname]      ,[password]      ,[groupid]      ,[olimg]      ,[adminid]      ,[invisible]      ,[action]      ,[lastactivity]      ,[lastposttime]      ,[lastpostpmtime]      ,[lastsearchtime]      ,[lastupdatetime]      ,[forumid]      ,[forumname]      ,[titleid]      ,[title]      ,[verifycode]      ,[newpms]      ,[newnotices] FROM [{0}online] WHERE [userid]=@userid AND [ip]=@ip  LIMIT 1", BaseConfigs.GetTablePrefix), parms).Tables[0];
        }

        /// <summary>
        /// ��������û���֤���Ƿ���Ч
        /// </summary>
        /// <param name="olid">�����û�ID</param>
        /// <param name="verifycode">��֤��</param>
        /// <returns>�����û�ID</returns>
        public bool CheckUserVerifyCode(int olId, string verifyCode, string newVerifyCode)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@olid", (DbType)NpgsqlDbType.Integer, 4, olId),
                                        DbHelper.MakeInParam("@verifycode", (DbType)NpgsqlDbType.Varchar, 10, verifyCode)
                                    };
            string commandText = string.Format("SELECT  [olid] FROM [{0}online] WHERE [olid]=@olid and [verifycode]=@verifycode   LIMIT 1",
                                                BaseConfigs.GetTablePrefix);
            DataTable dt = DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
            parms[1].Value = newVerifyCode;

            commandText = string.Format("UPDATE [{0}online] SET [verifycode]=@verifycode WHERE [olid]=@olid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
            return dt.Rows.Count > 0;
        }

        /// <summary>
        /// �����û�����״̬
        /// </summary>
        /// <param name="uid">�û�Id</param>
        /// <param name="onlinestate">����״̬��1����</param>
        /// <returns></returns>
        public int SetUserOnlineState(int uid, int onlineState)
        {
            string commandText = string.Format("UPDATE [{0}users] SET [onlinestate]={1},[lastactivity]=NOW(),[lastvisit]=NOW() WHERE [uid]={2}",
                                                BaseConfigs.GetTablePrefix,
                                                onlineState,
                                                uid);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// ɾ������������һ�������û���Ϣ
        /// </summary>
        /// <returns>ɾ������</returns>
        public int DeleteRowsByIP(string ip)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@ip",(DbType)NpgsqlDbType.Varchar,15,ip)
								   };

            //�����߱����𵽱�����ݿ�ʱ
            if (EntLibConfigs.GetConfig() != null)
            {
                OnlineTableConnect onlineTableConnect = EntLibConfigs.GetConfig().Onlinetableconnect;
                if (onlineTableConnect.Enable && !string.IsNullOrEmpty(onlineTableConnect.SqlServerConn))
                {   //�ֽ������SQL��䣬���⵱online�����𵽱�Ļ�����ʱ���ִ��������
                    DbDataReader dataReader = DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT [userid] FROM [{0}online] WHERE [userid]>0 AND [ip]=@ip", BaseConfigs.GetTablePrefix), parms);
                    string uidlist = "";
                    while (dataReader.Read())
                        uidlist += TypeConverter.ObjectToInt(dataReader["userid"], 0) + ",";
                    dataReader.Close();

                    if (!string.IsNullOrEmpty(uidlist.TrimEnd(',')))
                        DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE [{0}users] SET [onlinestate]=0,[lastactivity]=NOW() WHERE [uid] IN ({1})", BaseConfigs.GetTablePrefix, uidlist.TrimEnd(',')));
                }
            }
            else
                DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE [{0}users] SET [onlinestate]=0,[lastactivity]=NOW() WHERE [uid] IN (SELECT [userid] FROM [{0}online] WHERE [userid]>0 AND [ip]=@ip)",
                                                BaseConfigs.GetTablePrefix), parms);

            if (ip != "0.0.0.0")
                return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("DELETE FROM [{0}online] WHERE [userid]=-1 AND [ip]=@ip", BaseConfigs.GetTablePrefix), parms);

            return 0;
        }

        /// <summary>
        /// ɾ�����߱���ָ������id����
        /// </summary>
        /// <param name="olid">����id</param>
        /// <returns></returns>
        public int DeleteRows(int olId)
        {
            string commandText = string.Format("DELETE FROM [{0}online] WHERE [olid]={1}", BaseConfigs.GetTablePrefix, olId);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// �����û��ĵ�ǰ�����������Ϣ
        /// </summary>
        /// <param name="olid">�����б�id</param>
        /// <param name="action">����</param>
        /// <param name="inid">����λ�ô���</param>
        public void UpdateAction(int olId, int action, int inid)
        {
            DbParameter[] parms = {
										   DbHelper.MakeInParam("@action",(DbType)NpgsqlDbType.Smallint,2,action),
                                           DbHelper.MakeInParam("@lastupdatetime", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
										   DbHelper.MakeInParam("@forumid",(DbType)NpgsqlDbType.Integer,4,inid),
										   DbHelper.MakeInParam("@forumname",(DbType)NpgsqlDbType.Varchar,100,""),
										   DbHelper.MakeInParam("@titleid",(DbType)NpgsqlDbType.Integer,4,inid),
										   DbHelper.MakeInParam("@title",(DbType)NpgsqlDbType.Varchar,160,""),
										   DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer,4,olId)
								  };
            StringBuilder sb = new StringBuilder();
            if (action == 5 || action == 6)
            {
                sb.Append("UPDATE [{0}online] SET [lastactivity]=[action],[action]=@action,[lastupdatetime]=@lastupdatetime,[lastposttime]= @lastupdatetime,[forumid]=@forumid,[forumname]=@forumname,[titleid]=@titleid,[title]=@title WHERE [olid]=@olid");
            }
            else
            {
                sb.Append("UPDATE [{0}online] SET [lastactivity]=[action],[action]=@action,[lastupdatetime]=@lastupdatetime,[forumid]=@forumid,[forumname]=@forumname,[titleid]=@titleid,[title]=@title WHERE [olid]=@olid");
            }

            DbHelper.ExecuteNonQuery(CommandType.Text, string.Format(sb.ToString(), BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �����û��ĵ�ǰ�����������Ϣ
        /// </summary>
        /// <param name="olid">�����б�id</param>
        /// <param name="action">����id</param>
        /// <param name="fid">���id</param>
        /// <param name="forumname">�����</param>
        /// <param name="tid">����id</param>
        /// <param name="topictitle">������</param>
        public void UpdateAction(int olId, int action, int fid, string forumName, int tid, string topicTitle)
        {
            DbParameter[] parms = {
										   DbHelper.MakeInParam("@action",(DbType)NpgsqlDbType.Smallint,2,action),
                                           DbHelper.MakeInParam("@lastupdatetime", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
										   DbHelper.MakeInParam("@forumid",(DbType)NpgsqlDbType.Integer,4,fid),
										   DbHelper.MakeInParam("@forumname",(DbType)NpgsqlDbType.Varchar,100,forumName),
										   DbHelper.MakeInParam("@titleid",(DbType)NpgsqlDbType.Integer,4,tid),
										   DbHelper.MakeInParam("@title",(DbType)NpgsqlDbType.Varchar,160,topicTitle),
										   DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer,4,olId)
								  };
            StringBuilder sb = new StringBuilder();
            if (action == 5 || action == 6)
            {
                sb.Append("UPDATE [{0}online] SET [lastactivity]=[action],[action]=@action,[lastupdatetime]=@lastupdatetime,[lastposttime]= @lastupdatetime,[forumid]=@forumid,[forumname]=@forumname,[titleid]=@titleid,[title]=@title WHERE [olid]=@olid");
            }
            else
            {
                sb.Append(" UPDATE [{0}online] SET [lastactivity]=[action],[action]=@action,[lastupdatetime]=@lastupdatetime,[forumid]=@forumid,[forumname]=@forumname,[titleid]=@titleid,[title]=@title WHERE [olid]=@olid");
            }

            DbHelper.ExecuteNonQuery(CommandType.Text, string.Format(sb.ToString(), BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �����û����ʱ��
        /// </summary>
        /// <param name="olid">����id</param>
        public void UpdateLastTime(int olId)
        {
            DbParameter[] parms = {
                                           DbHelper.MakeInParam("@lastupdatetime", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
										   DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer,4,olId)
									   };
            string commandText = string.Format("UPDATE [{0}online] SET [lastupdatetime]=@lastupdatetime WHERE [olid]=@olid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// �����û������ʱ��
        /// </summary>
        /// <param name="olid">����id</param>
        public void UpdatePostTime(int olId)
        {
            string commandText = string.Format("UPDATE [{0}online] SET [lastposttime]='{1}' WHERE [olid]={2}",
                                                BaseConfigs.GetTablePrefix,
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), olId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// �����û���󷢶���Ϣʱ��
        /// </summary>
        /// <param name="olid">����id</param>
        public void UpdatePostPMTime(int olId)
        {
            string commandText = string.Format("UPDATE [{0}online] SET [lastpostpmtime]='{1}' WHERE [olid]={2}",
                                                BaseConfigs.GetTablePrefix,
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), olId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// �������߱���ָ���û��Ƿ�����
        /// </summary>
        /// <param name="olid">����id</param>
        /// <param name="invisible">�Ƿ�����</param>
        public void UpdateInvisible(int olId, int invisible)
        {
            string commandText = string.Format("UPDATE [{0}online] SET [invisible]={1} WHERE [olid]={2}",
                                                BaseConfigs.GetTablePrefix,
                                                invisible,
                                                olId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// �������߱���ָ���û����û�����
        /// </summary>
        /// <param name="olid">����id</param>
        /// <param name="password">�û�����</param>
        public void UpdatePassword(int olId, string passWord)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@password",(DbType)NpgsqlDbType.Char,32,passWord),
									   DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer,4,olId)
								   };
            string commandText = string.Format("UPDATE [{0}online] SET [password]=@password WHERE [olid]=@olid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// �����û�IP��ַ
        /// </summary>
        /// <param name="olid">����id</param>
        /// <param name="ip">ip��ַ</param>
        public void UpdateIP(int olId, string ip)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@ip",(DbType)NpgsqlDbType.Varchar,15,ip),
									   DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer,4,olId)
								   };
            string commandText = string.Format("UPDATE [{0}online] SET [ip]=@ip WHERE [olid]=@olid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// �����û��������ʱ��
        /// </summary>
        /// <param name="olid">����id</param>
        public void UpdateSearchTime(int olId)
        {
            string commandText = string.Format("UPDATE [{0}online] SET [lastsearchtime]={1} WHERE [olid]={2}",
                                                BaseConfigs.GetTablePrefix,
                                                olId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// �����û����û���
        /// </summary>
        /// <param name="userId">�û�ID</param>
        /// <param name="groupid">����</param>
        public void UpdateGroupid(int userId, int groupId)
        {
            string commandText = string.Format("UPDATE [{0}online] SET [groupid]={1} WHERE [userid]={2}",
                                                BaseConfigs.GetTablePrefix,
                                                groupId,
                                                userId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        /// <summary>
        /// ���ָ��ID�Ķ���Ϣ������
        /// </summary>
        /// <param name="pmid">����Ϣpmid</param>
        /// <returns>����Ϣ����</returns>
        public IDataReader GetPrivateMessageInfo(int pmId)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@pmid", (DbType)NpgsqlDbType.Integer,4, pmId),
			                        };
            string commandText = string.Format("SELECT TOP 1 {0} FROM [{1}pms] WHERE [pmid]=@pmid",
                                                DbFields.PMS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteReader(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// ���ָ���û��Ķ���Ϣ�б�
        /// </summary>
        /// <param name="userId">�û�ID</param>
        /// <param name="folder">����Ϣ����(0:�ռ���,1:������,2:�ݸ���)</param>
        /// <param name="pagesize">ÿҳ��ʾ����Ϣ��</param>
        /// <param name="pageindex">��ǰҪ��ʾ��ҳ��</param>
        /// <param name="inttype">ɸѡ����1Ϊδ��</param>
        /// <returns>����Ϣ�б�</returns>
        public IDataReader GetPrivateMessageList(int userId, int folder, int pageSize, int pageIndex, int intType)
        {
            //DbParameter[] parms = {
            //                           DbHelper.MakeInParam("@userid",(DbType)NpgsqlDbType.Integer,4,userId),
            //                           DbHelper.MakeInParam("@folder",(DbType)NpgsqlDbType.Integer,4,folder),
            //                           DbHelper.MakeInParam("@pagesize", (DbType)NpgsqlDbType.Integer,4,pageSize),
            //                           DbHelper.MakeInParam("@pageindex",(DbType)NpgsqlDbType.Integer,4,pageIndex),
            //                           DbHelper.MakeInParam("@inttype",(DbType)NpgsqlDbType.Varchar,500,intType)

            //                      };



            //return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("{0}getpmlist", BaseConfigs.GetTablePrefix), parms);
            string strwhere = "";
            if (intType == 1)
            {
                strwhere = "[new]=1";
            }

            DbParameter[] prams = {
                         
                                       DbHelper.MakeInParam("@strwhere",(DbType)NpgsqlDbType.Varchar,500,strwhere)

                                   };

            string strsql;
            string msgformortoid = "msgtoid";
            if (folder == 1 || folder == 2)
            {

                msgformortoid = "msgfromid";
            }

            //if (pageindex == 1)
            //{
            if (strwhere != "")
            {
                strsql = "SELECT [pmid],[msgfrom],[msgfromid],[msgto],[msgtoid],[folder],[new],[subject],[postdatetime],[message] FROM [" + BaseConfigs.GetTablePrefix + "pms] WHERE [" + msgformortoid + "]= " + userId + "  AND [folder]= " + folder + "  AND  " + strwhere + " ORDER BY [pmid] DESC LIMIT " + ((pageIndex - 1) * pageSize).ToString() + "OFFSET" + pageSize;
            }
            else
            {
                strsql = "SELECT [pmid],[msgfrom],[msgfromid],[msgto],[msgtoid],[folder],[new],[subject],[postdatetime],[message] FROM [" + BaseConfigs.GetTablePrefix + "pms] WHERE [" + msgformortoid + "]= " + userId + "  AND [folder]= " + folder + "  ORDER BY [pmid] DESC LIMIT " + ((pageIndex - 1) * pageSize).ToString() + "  OFFSET " + pageSize;

            }



            return DbHelper.ExecuteReader(CommandType.Text, strsql, prams);

        }

        /// <summary>
        /// �õ����û��Ķ���Ϣ����
        /// </summary>
        /// <param name="userId">�û�ID</param>
        /// <param name="folder">�����ļ���(0:�ռ���,1:������,2:�ݸ���)</param>
        /// <param name="state">����Ϣ״̬(0:�Ѷ�����Ϣ��1:δ������Ϣ��-1:ȫ������Ϣ)</param>
        /// <returns>����Ϣ����</returns>
        public int GetPrivateMessageCount(int userId, int folder, int state)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@userid",(DbType)NpgsqlDbType.Integer,4,userId),
									   DbHelper.MakeInParam("@folder",(DbType)NpgsqlDbType.Integer,4,folder),								   
									   DbHelper.MakeInParam("@state",(DbType)NpgsqlDbType.Integer,4,state)
								   };

            StringBuilder sb = new StringBuilder();
            if (folder == -1)
            {
                sb.Append("	  SELECT COUNT(pmid) AS [pmcount] FROM [dnt_pms] WHERE ([msgtoid] = @userid AND [folder]=0) OR ([msgfromid] = @userid AND [folder] = 1) OR ([msgfromid] = @userid AND [folder] = 2)");
            }
            else
            {
                if (folder == 0)
                {

                    if (state == -1)
                    {
                        sb.Append("	SELECT COUNT(pmid) AS [pmcount] FROM [dnt_pms] WHERE [msgtoid] = @userid AND [folder] = @folder");
                    }
                    else if (state == 2)
                    {

                        sb.Append("	SELECT COUNT(pmid) AS [pmcount] FROM [dnt_pms] WHERE [msgtoid] = @userid AND [folder] = @folder AND [new]=1 AND GETDATE()-[postdatetime]<3");
                    }
                    else
                    {
                        sb.Append("SELECT COUNT(pmid) AS [pmcount] FROM [dnt_pms] WHERE [msgtoid] = @userid AND [folder] = @folder AND [new] = @state");
                    }
                }
                else
                {

                    if (state == -1)
                    {
                        sb.Append("SELECT COUNT(pmid) AS [pmcount] FROM [dnt_pms] WHERE [msgfromid] = @userid AND [folder] = @folder");
                    }
                    else if (state == 2)
                    {
                        sb.Append("SELECT COUNT(pmid) AS [pmcount] FROM [dnt_pms] WHERE [msgfromid] = @userid AND [folder] = @folder AND [new]=1 AND GETDATE()-[postdatetime]<3");
                    }
                    else
                    {
                        sb.Append("	SELECT COUNT(pmid) AS [pmcount] FROM [dnt_pms] WHERE [msgfromid] = @userid AND [folder] = @folder AND [new] = @state");
                    }
                }
            }

            return TypeConverter.ObjectToInt(
                                 DbHelper.ExecuteScalar(CommandType.Text,
                                                        sb.ToString(),
                                                        parms));
        }

        /// <summary>
        /// �õ�������Ϣ����
        /// </summary>
        /// <returns>������Ϣ����</returns>
        public int GetAnnouncePrivateMessageCount()
        {
            return TypeConverter.ObjectToInt(
                         DbHelper.ExecuteScalarInMasterDB(CommandType.Text,
                                                             string.Format("SELECT COUNT(pmid) FROM [{0}pms] WHERE [msgtoid] = 0", BaseConfigs.GetTablePrefix)));
        }


        /// <summary>
        /// ���ָ���û��Ķ���Ϣ�б�
        /// </summary>
        /// <param name="pagesize">ÿҳ��ʾ����Ϣ��,Ϊ-1ʱ����ȫ��</param>
        /// <param name="pageindex">��ǰҪ��ʾ��ҳ��</param>
        /// <returns>����Ϣ�б�</returns>
        public IDataReader GetAnnouncePrivateMessageList(int pageSize, int pageIndex)
        {
            string commandText = "";
            if (pageSize == -1)
                commandText = string.Format("SELECT {0} FROM [{1}pms] WHERE [msgtoid] = 0 ORDER BY [pmid] DESC",
                                             DbFields.PMS, BaseConfigs.GetTablePrefix);
            else if (pageIndex <= 1)
                commandText = string.Format("SELECT TOP {0} {1} FROM [{2}pms] WHERE [msgtoid] = 0  ORDER BY [pmid] DESC",
                                             pageSize, DbFields.PMS, BaseConfigs.GetTablePrefix);
            else
                commandText = string.Format("SELECT TOP {0} {1} FROM [{2}pms] WHERE [msgtoid] = 0 AND [pmid] < (SELECT MIN([pmid]) FROM (SELECT TOP {3} [pmid] FROM [{2}pms] WHERE [msgtoid] = 0  ORDER BY [pmid] DESC) AS tblTmp)  ORDER BY [pmid] DESC",
                                             pageSize, DbFields.PMS, BaseConfigs.GetTablePrefix, (pageIndex - 1) * pageSize);

            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        /// <summary>
        /// ��������Ϣ
        /// </summary>
        /// <param name="__privatemessageinfo">����Ϣ����</param>
        /// <param name="savetosentbox">���ö���Ϣ�Ƿ��ڷ����䱣��(0Ϊ������, 1Ϊ����)</param>
        /// <returns>����Ϣ�����ݿ��е�pmid</returns>
        public int CreatePrivateMessage(PrivateMessageInfo privateMessageInfo, int saveToSentBox)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@pmid",(DbType)NpgsqlDbType.Integer,4,privateMessageInfo.Pmid),
									   DbHelper.MakeInParam("@msgfrom",(DbType)NpgsqlDbType.Varchar,20,privateMessageInfo.Msgfrom),
                                          DbHelper.MakeInParam("@msgto",(DbType)NpgsqlDbType.Varchar,20,privateMessageInfo.Msgto),
									   DbHelper.MakeInParam("@msgfromid",(DbType)NpgsqlDbType.Integer,4,privateMessageInfo.Msgfromid),
									
									   DbHelper.MakeInParam("@msgtoid",(DbType)NpgsqlDbType.Integer,4,privateMessageInfo.Msgtoid),
									   DbHelper.MakeInParam("@folder",(DbType)NpgsqlDbType.Smallint,2,privateMessageInfo.Folder),
									   DbHelper.MakeInParam("@new",(DbType)NpgsqlDbType.Integer,4,privateMessageInfo.New),
									   DbHelper.MakeInParam("@subject",(DbType)NpgsqlDbType.Varchar,80,privateMessageInfo.Subject),
									   DbHelper.MakeInParam("@postdatetime",(DbType)NpgsqlDbType.Timestamp,8,DateTime.Parse(privateMessageInfo.Postdatetime)),
									   DbHelper.MakeInParam("@message",(DbType)NpgsqlDbType.Text,0,privateMessageInfo.Message),
									   DbHelper.MakeInParam("@savetosentbox",(DbType)NpgsqlDbType.Integer,4,saveToSentBox)
								   };

            StringBuilder sb = new StringBuilder();
            if (privateMessageInfo.Folder != 0)
            {
                privateMessageInfo.Msgfrom = privateMessageInfo.Msgto;

            }
            else
            {
                sb.Append("UPDATE [dnt_users] SET [newpmcount]=ABS(COALESCE([newpmcount],0)*1)+1,[newpm] = 1 WHERE [uid]=@msgtoid;");
            }

            sb.Append("INSERT INTO [dnt_pms] 	([msgfrom],[msgfromid],[msgto],[msgtoid],[folder],[new],[subject],[postdatetime],[message])VALUES	(@msgfrom,@msgfromid,@msgto,@msgtoid,@folder,@new,@subject,@postdatetime,@message) ;");
            sb.Append("select currval('dnt_pms_pmid_seq');");
            if (privateMessageInfo.Folder == 0 && saveToSentBox == 1)
            {
                sb.Append("INSERT INTO [dnt_pms]			([msgfrom],[msgfromid],[msgto],[msgtoid],[folder],[new],[subject],[postdatetime],[message])		VALUES			(@msgfrom,@msgfromid,@msgto,@msgtoid,1,@new,@subject,@postdatetime,@message)");
            }
            return TypeConverter.ObjectToInt(
                                 DbHelper.ExecuteScalar(CommandType.Text,
                                                        string.Format(sb.ToString()), parms), -1);
        }

        /// <summary>
        /// ɾ��ָ���û��Ķ���Ϣ
        /// </summary>
        /// <param name="userId">�û�ID</param>
        /// <param name="pmitemid">Ҫɾ���Ķ���Ϣ�б�(����)</param>
        /// <returns>ɾ����¼��</returns>
        public int DeletePrivateMessages(int userId, string pmIdList)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@userid", (DbType)NpgsqlDbType.Integer,4, userId)
			                      };
            string commandText = string.Format("DELETE FROM [{0}pms] WHERE [pmid] IN ({1}) AND ([msgtoid] = @userid OR [msgfromid] = @userid)",
                                                BaseConfigs.GetTablePrefix,
                                                pmIdList);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// ����¶���Ϣ��
        /// </summary>
        /// <returns></returns>
        public int GetNewPMCount(int userId)
        {
            string commandText = string.Format("SELECT COUNT([pmid]) AS [pmcount] FROM [{0}pms] WHERE [new] = 1 AND [folder] = 0 AND [msgtoid] = {1}",
                                                BaseConfigs.GetTablePrefix,
                                                userId);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText));
        }

        /// <summary>
        /// ɾ��ָ���û���һ������Ϣ
        /// </summary>
        /// <param name="userId">�û�id</param>
        /// <param name="pmid">pmid</param>
        /// <returns></returns>
        public int DeletePrivateMessage(int userId, int pmId)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@userid", (DbType)NpgsqlDbType.Integer,4, userId),
									   DbHelper.MakeInParam("@pmid", (DbType)NpgsqlDbType.Integer,4, pmId)
			                      };
            string commandText = string.Format("DELETE FROM [{0}pms] WHERE [pmid]=@pmid AND ([msgtoid] = @userid OR [msgfromid] = @userid)",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// ���ö���Ϣ״̬
        /// </summary>
        /// <param name="pmid">����ϢID</param>
        /// <param name="state">״ֵ̬</param>
        /// <returns>���¼�¼��</returns>
        public int SetPrivateMessageState(int pmId, byte state)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@pmid", (DbType)NpgsqlDbType.Integer,1,pmId),
									   DbHelper.MakeInParam("@state",(DbType)NpgsqlDbType.Smallint,1,state)
								   };
            string commandText = string.Format("UPDATE [{0}pms] SET [new]=@state WHERE [pmid]=@pmid", BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }


        public string GetUserGroupsStr()
        {
            return string.Format("SELECT [groupid], [grouptitle] FROM [{0}usergroups] ORDER BY [groupid]", BaseConfigs.GetTablePrefix);
        }


        public DataTable GetUserListByGroupid(string groupIdList)
        {
            DbParameter parm = DbHelper.MakeInParam("@groupIdList", (DbType)NpgsqlDbType.Varchar, 500, groupIdList);
            return DbHelper.ExecuteDataset(CommandType.StoredProcedure, string.Format("{0}getuserlistbygroupid", BaseConfigs.GetTablePrefix), parm).Tables[0];
        }

        //�����ں�̨���Ͷ���Ϣʱʹ��
        public DataTable GetUserListByGroupid(string groupIdList, int topNumber, int start_uid)
        {
            return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT   [uid],[username],[email] FROM [{1}users] WHERE [groupid] IN ({2}) And [uid] > {3} ORDER BY [uid] ASC  limit {0}", topNumber, BaseConfigs.GetTablePrefix, groupIdList, start_uid)).Tables[0];
        }

        public DataTable GetUserNameByUid(int uid)
        {
            string commandText = string.Format("SELECT  [username] FROM [{0}users] WHERE [uid]={1} limit 1",
                                                BaseConfigs.GetTablePrefix,
                                                uid);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        public string GetSystemGroupInfoSql()
        {
            return string.Format("SELECT {0} FROM [{1}usergroups] WHERE [groupid]<=8 ORDER BY [groupid]",
                                  DbFields.USER_GROUPS,
                                  BaseConfigs.GetTablePrefix);
        }

        public void UpdateUserCredits(int uid)
        {
            //string commandText = string.Format("UPDATE [{0}users] SET [credits] = {1} WHERE [uid]={2}", 
            //                                    BaseConfigs.GetTablePrefix, 
            //                                    credits, 
            //                                    uid);
            //DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
            DbParameter parm = DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 1, uid);
            DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE [{0}users] SET [credits] = extcredits1 + posts + digestposts* 5 WHERE [uid] = @uid", BaseConfigs.GetTablePrefix), parm);
        }

        public bool CheckUserCreditsIsEnough(int uid, float[] values)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@extcredits1", (DbType)NpgsqlDbType.Real, 8, values[0]),
									   DbHelper.MakeInParam("@extcredits2", (DbType)NpgsqlDbType.Real, 8, values[1]),
									   DbHelper.MakeInParam("@extcredits3", (DbType)NpgsqlDbType.Real, 8, values[2]),
									   DbHelper.MakeInParam("@extcredits4", (DbType)NpgsqlDbType.Real, 8, values[3]),
									   DbHelper.MakeInParam("@extcredits5", (DbType)NpgsqlDbType.Real, 8, values[4]),
									   DbHelper.MakeInParam("@extcredits6", (DbType)NpgsqlDbType.Real, 8, values[5]),
									   DbHelper.MakeInParam("@extcredits7", (DbType)NpgsqlDbType.Real, 8, values[6]),
									   DbHelper.MakeInParam("@extcredits8", (DbType)NpgsqlDbType.Real, 8, values[7])
								   };
            string commandText = string.Format("SELECT COUNT(1) FROM [{0}users] WHERE [uid]=@uid AND"
                                                + "	[extcredits1]>= (case when @extcredits1<0 then abs(@extcredits1) else [extcredits1] end) AND "
                                                + "	[extcredits2]>= (case when @extcredits2<0 then abs(@extcredits2) else [extcredits2] end) AND "
                                                + "	[extcredits3]>= (case when @extcredits3<0 then abs(@extcredits3) else [extcredits3] end) AND "
                                                + "	[extcredits4]>= (case when @extcredits4<0 then abs(@extcredits4) else [extcredits4] end) AND "
                                                + "	[extcredits5]>= (case when @extcredits5<0 then abs(@extcredits5) else [extcredits5] end) AND "
                                                + "	[extcredits6]>= (case when @extcredits6<0 then abs(@extcredits6) else [extcredits6] end) AND "
                                                + "	[extcredits7]>= (case when @extcredits7<0 then abs(@extcredits7) else [extcredits7] end) AND "
                                                + "	[extcredits8]>= (case when @extcredits8<0 then abs(@extcredits8) else [extcredits8] end) ",
                                                BaseConfigs.GetTablePrefix);

            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText, parms)) != 0;
        }

        public void UpdateUserCredits(int uid, float[] values)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@extcredits1", (DbType)NpgsqlDbType.Real, 8, values[0]),
									   DbHelper.MakeInParam("@extcredits2", (DbType)NpgsqlDbType.Real, 8, values[1]),
									   DbHelper.MakeInParam("@extcredits3", (DbType)NpgsqlDbType.Real, 8, values[2]),
									   DbHelper.MakeInParam("@extcredits4", (DbType)NpgsqlDbType.Real, 8, values[3]),
									   DbHelper.MakeInParam("@extcredits5", (DbType)NpgsqlDbType.Real, 8, values[4]),
									   DbHelper.MakeInParam("@extcredits6", (DbType)NpgsqlDbType.Real, 8, values[5]),
									   DbHelper.MakeInParam("@extcredits7", (DbType)NpgsqlDbType.Real, 8, values[6]),
									   DbHelper.MakeInParam("@extcredits8", (DbType)NpgsqlDbType.Real, 8, values[7])
								   };

            string commandText = string.Format("UPDATE [{0}users] SET "
                                                + "		[extcredits1]=[extcredits1] + @extcredits1, "
                                                + "		[extcredits2]=[extcredits2] + @extcredits2, "
                                                + "		[extcredits3]=[extcredits3] + @extcredits3, "
                                                + "		[extcredits4]=[extcredits4] + @extcredits4, "
                                                + "		[extcredits5]=[extcredits5] + @extcredits5, "
                                                + "		[extcredits6]=[extcredits6] + @extcredits6, "
                                                + "		[extcredits7]=[extcredits7] + @extcredits7, "
                                                + "		[extcredits8]=[extcredits8] + @extcredits8  WHERE [uid]=@uid",
                                                BaseConfigs.GetTablePrefix);

            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public bool CheckUserCreditsIsEnough(int uid, float[] values, int pos, int mount)
        {
            //���ּ����ֲ���ʱ
            if (pos < 0)
            {
                DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@extcredits1", (DbType)NpgsqlDbType.Real, 8, values[0] * mount),
									   DbHelper.MakeInParam("@extcredits2", (DbType)NpgsqlDbType.Real, 8, values[1] * mount),
									   DbHelper.MakeInParam("@extcredits3", (DbType)NpgsqlDbType.Real, 8, values[2] * mount),
									   DbHelper.MakeInParam("@extcredits4", (DbType)NpgsqlDbType.Real, 8, values[3] * mount),
									   DbHelper.MakeInParam("@extcredits5", (DbType)NpgsqlDbType.Real, 8, values[4] * mount),
									   DbHelper.MakeInParam("@extcredits6", (DbType)NpgsqlDbType.Real, 8, values[5] * mount),
									   DbHelper.MakeInParam("@extcredits7", (DbType)NpgsqlDbType.Real, 8, values[6] * mount),
									   DbHelper.MakeInParam("@extcredits8", (DbType)NpgsqlDbType.Real, 8, values[7] * mount)
								   };
                string commandText = string.Format("SELECT COUNT(1) FROM [{0}users] WHERE [uid]=@uid AND"
                                                    + "	[extcredits1]>= (case when @extcredits1 < 0 then abs(@extcredits1) else 0 end) AND "
                                                    + "	[extcredits2]>= (case when @extcredits2 < 0 then abs(@extcredits2) else 0 end) AND "
                                                    + "	[extcredits3]>= (case when @extcredits3 < 0 then abs(@extcredits3) else 0 end) AND "
                                                    + "	[extcredits4]>= (case when @extcredits4 < 0 then abs(@extcredits4) else 0 end) AND "
                                                    + "	[extcredits5]>= (case when @extcredits5 < 0 then abs(@extcredits5) else 0 end) AND "
                                                    + "	[extcredits6]>= (case when @extcredits6 < 0 then abs(@extcredits6) else 0 end) AND "
                                                    + "	[extcredits7]>= (case when @extcredits7 < 0 then abs(@extcredits7) else 0 end) AND "
                                                    + "	[extcredits8]>= (case when @extcredits8 < 0 then abs(@extcredits8) else 0 end) ",
                                                    BaseConfigs.GetTablePrefix);
                return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText, parms)) != 0;
            }
            return true;
        }

        public void UpdateUserCredits(int uid, float[] values, int pos, int mount)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@extcredits1", (DbType)NpgsqlDbType.Real, 8, values[0] * pos * mount),
									   DbHelper.MakeInParam("@extcredits2", (DbType)NpgsqlDbType.Real, 8, values[1] * pos * mount),
									   DbHelper.MakeInParam("@extcredits3", (DbType)NpgsqlDbType.Real, 8, values[2] * pos * mount),
									   DbHelper.MakeInParam("@extcredits4", (DbType)NpgsqlDbType.Real, 8, values[3] * pos * mount),
									   DbHelper.MakeInParam("@extcredits5", (DbType)NpgsqlDbType.Real, 8, values[4] * pos * mount),
									   DbHelper.MakeInParam("@extcredits6", (DbType)NpgsqlDbType.Real, 8, values[5] * pos * mount),
									   DbHelper.MakeInParam("@extcredits7", (DbType)NpgsqlDbType.Real, 8, values[6] * pos * mount),
									   DbHelper.MakeInParam("@extcredits8", (DbType)NpgsqlDbType.Real, 8, values[7] * pos * mount)								   };
            if (pos < 0 && mount < 0)
            {
                for (int i = 1; i < parms.Length; i++)
                {
                    parms[i].Value = -Convert.ToInt32(parms[i].Value);
                }
            }

            string commandText = string.Format("UPDATE [{0}users] SET "
                                                + "	[extcredits1]=[extcredits1] + @extcredits1, "
                                                + "	[extcredits2]=[extcredits2] + @extcredits2, "
                                                + "	[extcredits3]=[extcredits3] + @extcredits3, "
                                                + "	[extcredits4]=[extcredits4] + @extcredits4, "
                                                + "	[extcredits5]=[extcredits5] + @extcredits5, "
                                                + "	[extcredits6]=[extcredits6] + @extcredits6, "
                                                + "	[extcredits7]=[extcredits7] + @extcredits7, "
                                                + "	[extcredits8]=[extcredits8] + @extcredits8 "
                                                + "WHERE [uid]=@uid",
                                                BaseConfigs.GetTablePrefix);

            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }


        public DataTable GetUserGroups()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}usergroups] ORDER BY [groupid]", DbFields.USER_GROUPS, BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDatasetInMasterDB(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetUserGroupRateRange(int groupId)
        {
            string commandText = string.Format("SELECT  [raterange] FROM [{0}usergroups] WHERE [groupid]={1} limit 1",
                                                BaseConfigs.GetTablePrefix,
                                                groupId);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public IDataReader GetUserTodayRate(int uid)
        {
            string commandText = string.Format("SELECT [extcredits], SUM(ABS([score])) AS [todayrate] FROM [{0}ratelog] WHERE DATEDIFF(d,[postdatetime],getdate()) = 0 AND [uid] = {1} GROUP BY [extcredits]",
                                                BaseConfigs.GetTablePrefix,
                                                uid);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        public DataTable GetSpecialUserGroup()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}usergroups] WHERE [radminid]=-1 AND [groupid]>8 ORDER BY [groupid]",
                                                DbFields.USER_GROUPS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }



        /// <summary>
        /// ����ָ���û�����Ϣ
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <returns>�û���Ϣ</returns>
        public IDataReader GetUserInfoToReader(int uid)
        {

            return DbHelper.ExecuteReader(CommandType.Text, string.Format(@"SELECT 
dnt_users.accessmasks, 
dnt_users.adminid,
dnt_users.avatarshowid,
dnt_users.bday,
dnt_users.credits,
dnt_users.digestposts,
dnt_users.email,
dnt_users.extcredits1,
dnt_users.extcredits2,
dnt_users.extcredits3,
dnt_users.extcredits4,
dnt_users.extcredits5,
dnt_users.extcredits6,
dnt_users.extcredits7,
dnt_users.extcredits8,
dnt_users.extgroupids,
dnt_users.gender,
dnt_users.groupexpiry,
dnt_users.groupid,
dnt_users.invisible,
dnt_users.joindate,
dnt_users.lastactivity,
dnt_users.lastip,
dnt_users.lastpost,
dnt_users.lastpostid,
dnt_users.lastposttitle,
dnt_users.lastvisit,
dnt_users.newpm,
dnt_users.newpmcount,
dnt_users.newsletter,
dnt_users.nickname,
dnt_users.oltime,
dnt_users.onlinestate,
dnt_users.pageviews,
dnt_users.password,
dnt_users.pmsound,
dnt_users.posts,
dnt_users.ppp,
dnt_users.regip,
dnt_users.secques,
dnt_users.showemail,
dnt_users.sigstatus,
dnt_users.spaceid,
dnt_users.templateid,
dnt_users.tpp,
dnt_users.uid,
dnt_users.username,
dnt_users.salt,
dnt_userfields.authflag,
dnt_userfields.authstr,
dnt_userfields.authtime,
dnt_userfields.avatar,
dnt_userfields.avatarheight,
dnt_userfields.avatarwidth,
dnt_userfields.bio,
dnt_userfields.customstatus,
dnt_userfields.icq,
dnt_userfields.idcard,
dnt_userfields.ignorepm,
dnt_userfields.location,
dnt_userfields.medals,
dnt_userfields.mobile,
dnt_userfields.msn,
dnt_userfields.phone,
dnt_userfields.qq,
dnt_userfields.realname,
dnt_userfields.sightml,
dnt_userfields.signature,
dnt_userfields.skype,
dnt_userfields.uid,
dnt_userfields.website,
dnt_userfields.yahoo
FROM dnt_users LEFT JOIN dnt_userfields ON dnt_users.uid=dnt_userfields.uid 
WHERE dnt_users.uid={0}   limit 1", uid));
        }

        ///<summary>
        /// ����IP�����û�
        /// </summary>
        /// <param name="ip">ip��ַ</param>
        /// <returns>�û���Ϣ</returns>
        public IDataReader GetUserInfoByIP(string ip)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@regip", (DbType)NpgsqlDbType.Char,15, ip),
			                      };
            return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT  [u].[uid], [u].[username], [u].[nickname], [u].[password], [u].[secques], [u].[spaceid], [u].[gender], [u].[adminid], [u].[groupid], [u].[groupexpiry], [u].[extgroupids], [u].[regip], [u].[joindate], [u].[lastip], [u].[lastvisit], [u].[lastactivity], [u].[lastpost], [u].[lastpostid], [u].[lastposttitle], [u].[posts], [u].[digestposts], [u].[oltime], [u].[pageviews], [u].[credits], [u].[extcredits1], [u].[extcredits2], [u].[extcredits3], [u].[extcredits4], [u].[extcredits5], [u].[extcredits6], [u].[extcredits7], [u].[extcredits8], [u].[avatarshowid], [u].[email], [u].[bday], [u].[sigstatus], [u].[tpp], [u].[ppp], [u].[templateid], [u].[pmsound], [u].[showemail], [u].[invisible], [u].[newpm], [u].[newpmcount], [u].[accessmasks], [u].[onlinestate], [u].[newsletter],[u].[salt], [uf].[website], [uf].[icq], [uf].[qq], [uf].[yahoo], [uf].[msn], [uf].[skype], [uf].[location], [uf].[customstatus], [uf].[avatar], [uf].[avatarwidth], [uf].[avatarheight], [uf].[medals], [uf].[bio], [uf].[signature], [uf].[sightml], [uf].[authstr], [uf].[authtime], [uf].[authflag], [uf].[realname], [uf].[idcard], [uf].[mobile], [uf].[phone], [uf].[ignorepm] FROM [dnt_users] [u] LEFT JOIN [dnt_userfields] [uf] ON [u].[uid]=[uf].[uid] WHERE [u].[regip]=@regip ORDER BY [u].[uid] DESC limit 1 ", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ��ȡ�û���Ϣ
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IDataReader GetUserInfoToReader(string userName)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Char,20, userName)
			                      };
            string commandText = string.Format("SELECT   {1} FROM [{0}users] AS [u] LEFT JOIN [{0}userfields] AS [uf] ON [u].[uid]=[uf].[uid] WHERE [u].[username]=@username  limit 1",
                                                BaseConfigs.GetTablePrefix,
                                                DbFields.USERS_JOIN_FIELDS);
            return DbHelper.ExecuteReader(CommandType.Text, commandText, parms);
        }
        /// <summary>
        /// ��ȡ�û���Ϣ
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IDataReader GetUserInfoToReader(int uid, string userName)
        {
            DbParameter[] parms = {
                                       DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4, uid),
									   DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Char,20, userName)
			                      };
            string commandText = string.Format("SELECT   {1} FROM [{0}users] AS [u] LEFT JOIN [{0}userfields] AS [uf] ON [u].[uid]=[uf].[uid] WHERE [u].[uid]=@uid AND [u].[username]=@username limit 1",
                                                BaseConfigs.GetTablePrefix,
                                                DbFields.USERS_JOIN_FIELDS);
            return DbHelper.ExecuteReader(CommandType.Text, commandText, parms);

        }
        /// <summary>
        /// ��ȡ����û���Ϣ
        /// </summary>
        /// <param name="uid">��id</param>
        /// <returns>�û������Ϣ</returns>
        public IDataReader GetShortUserInfoToReader(int uid)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer,4, uid),
			                      };
            return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT * FROM [{0}users] WHERE [uid]=@uid  limit 1", BaseConfigs.GetTablePrefix), parms);
        }


        public IDataReader GetUserName(int uid)
        {
            string commandText = string.Format("SELECT  [username] FROM [{0}users] WHERE [{0}users].[uid]={1} limit 1",
                                                BaseConfigs.GetTablePrefix,
                                                uid);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        public IDataReader GetUserJoinDate(int uid)
        {
            string commandText = string.Format("SELECT  [joindate] FROM [{0}users] WHERE [{0}users].[uid]={1} limit 1",
                                                BaseConfigs.GetTablePrefix,
                                                uid);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        public IDataReader GetShortUserInfoByName(string userName)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Varchar,20,userName),
			                      };
            string commandText = string.Format("SELECT   {0} FROM [{1}users] WHERE [{1}users].[username]=@username limit 1",
                                                DbFields.USERS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteReader(CommandType.Text, commandText, parms);
        }

        public DataTable GetUserList(int pageSize, int currentPage)
        {
            int pagetop = (currentPage - 1) * pageSize;
            string commandText;
            if (currentPage == 1)
                commandText = string.Format("SELECT  [{1}users].[uid], [{1}users].[username],[{1}users].[nickname], [{1}users].[joindate], [{1}users].[credits], [{1}users].[posts], [{1}users].[lastactivity], [{1}users].[email],[{1}users].[lastvisit],[{1}users].[lastvisit],[{1}users].[accessmasks], [{1}userfields].[location],[{1}usergroups].[grouptitle] FROM [{1}users] LEFT JOIN [{1}userfields] ON [{1}userfields].[uid] = [{1}users].[uid] LEFT JOIN [{1}usergroups] ON [{1}usergroups].[groupid]=[{1}users].[groupid] ORDER BY [{1}users].[uid] DESC limit {0}",
                                             pageSize,
                                             BaseConfigs.GetTablePrefix);
            else
                commandText = string.Format("SELECT TOP {0} [{1}users].[uid], [{1}users].[username],[{1}users].[nickname], [{1}users].[joindate], [{1}users].[credits], [{1}users].[posts], [{1}users].[lastactivity], [{1}users].[email],[{1}users].[lastvisit],[{1}users].[lastvisit],[{1}users].[accessmasks], [{1}userfields].[location],[{1}usergroups].[grouptitle] FROM [{1}users],[{1}userfields],[{1}usergroups] WHERE [{1}userfields].[uid] = [{1}users].[uid] AND  [{1}usergroups].[groupid]=[{1}users].[groupid]  ORDER BY [{1}users].[uid] DESC  limit {0} ,{2}",
                                             pageSize,
                                             BaseConfigs.GetTablePrefix,
                                             pagetop);

            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }

        /// <summary>
        /// ����û��б�DataTable
        /// </summary>
        /// <param name="pagesize">ÿҳ��¼��</param>
        /// <param name="pageindex">��ǰҳ��</param>
        /// <returns>�û��б�DataTable</returns>
        public DataTable GetUserList(int pageSize, int pageIndex, string column, string orderType)
        {
            string[] arrayorderby = new string[] { "username", "credits", "posts", "admin", "lastactivity", "joindate", "oltime" };
            int i = Array.IndexOf(arrayorderby, column);
            column = (i > 6 || i < 0) ? "uid" : arrayorderby[i];

            DbParameter[] parms = {
									   DbHelper.MakeInParam("@pagesize", (DbType)NpgsqlDbType.Integer,4,pageSize),
									   DbHelper.MakeInParam("@pageindex",(DbType)NpgsqlDbType.Integer,4,pageIndex),
									   DbHelper.MakeInParam("@column",(DbType)NpgsqlDbType.Varchar,1000,column),
                                       DbHelper.MakeInParam("@ordertype",(DbType)NpgsqlDbType.Varchar,5,orderType)
								   };
            StringBuilder sb = new StringBuilder();

            if (column == "uid")
            {
                column = "[dnt_users].[uid]";
            }
            else if (column == "admin")
            {
                column = "[adminid]";
            }
            sb.Append(@"SELECT  [dnt_users].[uid], 
									[dnt_users].[groupid],
									[dnt_users].[username], 
									[dnt_users].[nickname], 
									[dnt_users].[joindate], 
									[dnt_users].[credits], 
									[dnt_users].[posts], 
									[dnt_users].[lastactivity], 
									[dnt_users].[email], 
									[dnt_users].[oltime], 
									[dnt_userfields].[location] 
					FROM [dnt_users] LEFT JOIN [dnt_userfields] ON [dnt_userfields].[uid] = [dnt_users].[uid] ORDER BY  ");
            sb.Append(column);
            sb.Append("   ");
            sb.Append(orderType);

            sb.Append("   LIMIT ");
            sb.Append(pageSize);
            sb.Append(" OFFSET ");
            sb.Append((pageIndex - 1) * pageSize);


            return DbHelper.ExecuteDataset(CommandType.Text, sb.ToString(), parms).Tables[0];
        }

        /// <summary>
        /// ���Email�Ͱ�ȫ��
        /// </summary>
        /// <param name="username">�û���</param>
        /// <param name="email">email</param>
        /// <param name="questionid">����id</param>
        /// <param name="answer">��</param>
        /// <returns>�����ȷ�򷵻��û�id, ���򷵻�-1</returns>
        public IDataReader CheckEmailAndSecques(string userName, string email, string secques)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Char,20,userName),
									   DbHelper.MakeInParam("@email",(DbType)NpgsqlDbType.Char,50, email),
									   DbHelper.MakeInParam("@secques",(DbType)NpgsqlDbType.Char,8, secques)
								   };
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("{0}checkemailandsecques", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �������Ͱ�ȫ��
        /// </summary>
        /// <param name="username">�û���</param>
        /// <param name="password">����</param>
        /// <param name="originalpassword">�Ƿ��MD5����</param>
        /// <param name="questionid">����id</param>
        /// <param name="answer">��</param>
        /// <returns>�����ȷ�򷵻��û�id, ���򷵻�-1</returns>
        public IDataReader CheckPasswordAndSecques(string userName, string passWord, bool originalPassWord, string secques)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Char,20,userName),
									   DbHelper.MakeInParam("@password",(DbType)NpgsqlDbType.Char,32, originalPassWord ? Utils.MD5(passWord) : passWord),
									   DbHelper.MakeInParam("@secques",(DbType)NpgsqlDbType.Char,8, secques)
								   };
            return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT   [uid] FROM [{0}users] WHERE [username]=@username AND [password]=@password AND [secques]=@secques  LIMIT 1", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="username">�û���</param>
        /// <param name="password">����</param>
        /// <param name="originalpassword">�Ƿ��MD5����</param>
        /// <returns>�����ȷ�򷵻��û�id, ���򷵻�-1</returns>
        public IDataReader CheckPassword(string userName, string passWord, bool originalPassWord)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Char,20, userName),
									   DbHelper.MakeInParam("@password",(DbType)NpgsqlDbType.Char,32, originalPassWord ? Utils.MD5(passWord) : passWord)
								   };
            return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT   [uid], [groupid], [adminid] FROM [{0}users] WHERE [username]=@username AND [password]=@password LIMIT 1", BaseConfigs.GetTablePrefix), parms);

        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="password">����</param>
        /// <param name="originalpassword">�Ƿ��MD5����</param>
        /// <param name="groupid">�û���id</param>
        /// <param name="adminid">����id</param>
        /// <returns>����û�������ȷ�򷵻�uid, ���򷵻�-1</returns>
        public IDataReader CheckPassword(int uid, string passWord, bool originalPassWord)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid),
									   DbHelper.MakeInParam("@password",(DbType)NpgsqlDbType.Char,32, originalPassWord ? Utils.MD5(passWord) : passWord)
								   };
            return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT   [uid], [groupid], [adminid] FROM [dnt_users] WHERE [uid]=@uid AND [password]=@password  LIMIT 1", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ����ָ����email�����û��������û�uid
        /// </summary>
        /// <param name="email">email��ַ</param>
        /// <returns>�û�uid</returns>
        public IDataReader FindUserEmail(string email)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@email",(DbType)NpgsqlDbType.Char,50, email),
								   };
            return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT   [uid] FROM [dnt_users] WHERE [email]=@email limit 1", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �õ���̳���û�����
        /// </summary>
        /// <returns>�û�����</returns>
        public int GetUserCount()
        {
            string commandText = string.Format("SELECT COUNT(uid) FROM [{0}users]", BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText));
        }

        /// <summary>
        /// �õ���̳���û�����
        /// </summary>
        /// <returns>�û�����</returns>
        public int GetUserCountByAdmin()
        {
            string commandText = string.Format("SELECT COUNT(uid) FROM [{0}users] WHERE [{0}users].[adminid] > 0", BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText));
        }

        /// <summary>
        /// �������û�.
        /// </summary>
        /// <param name="__userinfo">�û���Ϣ</param>
        /// <returns>�����û�ID, ����Ѵ��ڸ��û����򷵻�-1</returns>
        public int CreateUser(UserInfo userInfo)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Char,20,userInfo.Username),
									   DbHelper.MakeInParam("@nickname",(DbType)NpgsqlDbType.Char,20,userInfo.Nickname),
									   DbHelper.MakeInParam("@password",(DbType)NpgsqlDbType.Char,32,userInfo.Password),
									   DbHelper.MakeInParam("@secques",(DbType)NpgsqlDbType.Char,8,userInfo.Secques),
									   DbHelper.MakeInParam("@gender",(DbType)NpgsqlDbType.Integer,4,userInfo.Gender),
									   DbHelper.MakeInParam("@adminid",(DbType)NpgsqlDbType.Integer,4,userInfo.Adminid),
									   DbHelper.MakeInParam("@groupid",(DbType)NpgsqlDbType.Smallint,2,userInfo.Groupid),
									   DbHelper.MakeInParam("@groupexpiry",(DbType)NpgsqlDbType.Integer,4,userInfo.Groupexpiry),
									   DbHelper.MakeInParam("@extgroupids",(DbType)NpgsqlDbType.Char,60,userInfo.Extgroupids),
									   DbHelper.MakeInParam("@regip",(DbType)NpgsqlDbType.Varchar,0,userInfo.Regip),
									   DbHelper.MakeInParam("@joindate",(DbType)NpgsqlDbType.Timestamp,0,DateTime.Parse( userInfo.Joindate)),
									   DbHelper.MakeInParam("@lastip",(DbType)NpgsqlDbType.Char,15,userInfo.Lastip),
									   DbHelper.MakeInParam("@lastvisit",(DbType)NpgsqlDbType.Timestamp,0,DateTime.Parse( userInfo.Lastvisit)),
									   DbHelper.MakeInParam("@lastactivity",(DbType)NpgsqlDbType.Timestamp,0,DateTime.Parse(userInfo.Lastactivity)),
									   DbHelper.MakeInParam("@lastpost",(DbType)NpgsqlDbType.Timestamp,0,DateTime.Parse(userInfo.Lastpost)),
									   DbHelper.MakeInParam("@lastpostid",(DbType)NpgsqlDbType.Integer,4,userInfo.Lastpostid),
									   DbHelper.MakeInParam("@lastposttitle",(DbType)NpgsqlDbType.Varchar,0,userInfo.Lastposttitle),
									   DbHelper.MakeInParam("@posts",(DbType)NpgsqlDbType.Integer,4,userInfo.Posts),
									   DbHelper.MakeInParam("@digestposts",(DbType)NpgsqlDbType.Smallint,2,userInfo.Digestposts),
									   DbHelper.MakeInParam("@oltime",(DbType)NpgsqlDbType.Integer,2,userInfo.Oltime),
									   DbHelper.MakeInParam("@pageviews",(DbType)NpgsqlDbType.Integer,4,userInfo.Pageviews),
									   DbHelper.MakeInParam("@credits",(DbType)NpgsqlDbType.Integer,4,userInfo.Credits),
									   DbHelper.MakeInParam("@extcredits1",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits1),
									   DbHelper.MakeInParam("@extcredits2",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits2),
									   DbHelper.MakeInParam("@extcredits3",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits3),
									   DbHelper.MakeInParam("@extcredits4",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits4),
									   DbHelper.MakeInParam("@extcredits5",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits5),
									   DbHelper.MakeInParam("@extcredits6",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits6),
									   DbHelper.MakeInParam("@extcredits7",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits7),
									   DbHelper.MakeInParam("@extcredits8",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits8),
									   DbHelper.MakeInParam("@avatarshowid",(DbType)NpgsqlDbType.Integer,4,0),
									   DbHelper.MakeInParam("@email",(DbType)NpgsqlDbType.Char,50,userInfo.Email),
									   DbHelper.MakeInParam("@bday",(DbType)NpgsqlDbType.Varchar,0,userInfo.Bday),
									   DbHelper.MakeInParam("@sigstatus",(DbType)NpgsqlDbType.Integer,4,userInfo.Sigstatus),
                                        DbHelper.MakeInParam("@salt",(DbType)NpgsqlDbType.Char,6,userInfo.Salt),
									   DbHelper.MakeInParam("@tpp",(DbType)NpgsqlDbType.Integer,4,userInfo.Tpp),
									   DbHelper.MakeInParam("@ppp",(DbType)NpgsqlDbType.Integer,4,userInfo.Ppp),
									   DbHelper.MakeInParam("@templateid",(DbType)NpgsqlDbType.Smallint,2,userInfo.Templateid),
									   DbHelper.MakeInParam("@pmsound",(DbType)NpgsqlDbType.Integer,4,userInfo.Pmsound),
									   DbHelper.MakeInParam("@showemail",(DbType)NpgsqlDbType.Integer,4,userInfo.Showemail),
									   DbHelper.MakeInParam("@newsletter",(DbType)NpgsqlDbType.Integer,4,userInfo.Newsletter),
									   DbHelper.MakeInParam("@invisible",(DbType)NpgsqlDbType.Integer,4,userInfo.Invisible),
									   DbHelper.MakeInParam("@newpm",(DbType)NpgsqlDbType.Integer,4,userInfo.Newpm),
									   DbHelper.MakeInParam("@accessmasks",(DbType)NpgsqlDbType.Integer,4,userInfo.Accessmasks),
                                      
									   DbHelper.MakeInParam("@website",(DbType)NpgsqlDbType.Varchar,80,userInfo.Website),
									   DbHelper.MakeInParam("@icq",(DbType)NpgsqlDbType.Varchar,12,userInfo.Icq),
									   DbHelper.MakeInParam("@qq",(DbType)NpgsqlDbType.Varchar,12,userInfo.Qq),
									   DbHelper.MakeInParam("@yahoo",(DbType)NpgsqlDbType.Varchar,40,userInfo.Yahoo),
									   DbHelper.MakeInParam("@msn",(DbType)NpgsqlDbType.Varchar,40,userInfo.Msn),
									   DbHelper.MakeInParam("@skype",(DbType)NpgsqlDbType.Varchar,40,userInfo.Skype),
									   DbHelper.MakeInParam("@location",(DbType)NpgsqlDbType.Varchar,30,userInfo.Location),
									   DbHelper.MakeInParam("@customstatus",(DbType)NpgsqlDbType.Varchar,30,userInfo.Customstatus),
									   DbHelper.MakeInParam("@avatar",(DbType)NpgsqlDbType.Varchar,255,""),
									   DbHelper.MakeInParam("@avatarwidth",(DbType)NpgsqlDbType.Integer,4,0),
									   DbHelper.MakeInParam("@avatarheight",(DbType)NpgsqlDbType.Integer,4,0),
									   DbHelper.MakeInParam("@medals",(DbType)NpgsqlDbType.Varchar,40, userInfo.Medals),
									   DbHelper.MakeInParam("@bio",(DbType)NpgsqlDbType.Varchar,500,userInfo.Bio),
									   DbHelper.MakeInParam("@signature",(DbType)NpgsqlDbType.Varchar,500,userInfo.Signature),
									   DbHelper.MakeInParam("@sightml",(DbType)NpgsqlDbType.Varchar,1000,userInfo.Sightml),
									   DbHelper.MakeInParam("@authstr",(DbType)NpgsqlDbType.Varchar,20,userInfo.Authstr),
                                       DbHelper.MakeInParam("@realname",(DbType)NpgsqlDbType.Varchar,10,userInfo.Realname),
                                       DbHelper.MakeInParam("@idcard",(DbType)NpgsqlDbType.Varchar,20,userInfo.Idcard),
                                       DbHelper.MakeInParam("@mobile",(DbType)NpgsqlDbType.Varchar,20,userInfo.Mobile),
                                       DbHelper.MakeInParam("@phone",(DbType)NpgsqlDbType.Varchar,20,userInfo.Phone),
                                       DbHelper.MakeInParam("@phone",(DbType)NpgsqlDbType.Integer,0,0)
								   };

            string sql = @"INSERT  INTO [dnt_users]
            ( [username] ,
              [nickname] ,
              [password] ,
              [secques] ,
              [gender] ,
              [adminid] ,
              [groupid] ,
              [groupexpiry] ,
              [extgroupids] ,
              [regip] ,
              [joindate] ,
              [lastip] ,
              [lastvisit] ,
              [lastactivity] ,
              [lastpost] ,
              [lastpostid] ,
              [lastposttitle] ,
              [posts] ,
              [digestposts] ,
              [oltime] ,
              [pageviews] ,
              [credits] ,
              [extcredits1] ,
              [extcredits2] ,
              [extcredits3] ,
              [extcredits4] ,
              [extcredits5] ,
              [extcredits6] ,
              [extcredits7] ,
              [extcredits8] ,
              [avatarshowid] ,
              [email] ,
              [bday] ,
              [sigstatus] ,
              [salt] ,
              [tpp] ,
              [ppp] ,
              [templateid] ,
              [pmsound] ,
              [showemail] ,
              [newsletter] ,
              [invisible] ,
              [newpm] ,
              [accessmasks]
            )
    VALUES  ( @username ,
              @nickname ,
              @password ,
              @secques ,
              @gender ,
              @adminid ,
              @groupid ,
              @groupexpiry ,
              @extgroupids ,
              @regip ,
              @joindate ,
              @lastip ,
              @lastvisit ,
              @lastactivity ,
              @lastpost ,
              @lastpostid ,
              @lastposttitle ,
              @posts ,
              @digestposts ,
              @oltime ,
              @pageviews ,
              @credits ,
              @extcredits1 ,
              @extcredits2 ,
              @extcredits3 ,
              @extcredits4 ,
              @extcredits5 ,
              @extcredits6 ,
              @extcredits7 ,
              @extcredits8 ,
              @avatarshowid ,
              @email ,
              @bday ,
              @sigstatus ,
              @salt ,
              @tpp ,
              @ppp ,
              @templateid ,
              @pmsound ,
              @showemail ,
              @newsletter ,
              @invisible ,
              @newpm ,
              @accessmasks
            )";



            int uid = TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text,
                                                                  sql,
                                                                 parms), -1);
            if (uid != -1)
            {
                //
                parms[parms.Length - 1].Value = uid;
                string sql2 = @"  UPDATE  [dnt_statistics]
            SET     [totalusers] = [totalusers] + 1 ,
                    [lastusername] = @username ,
                    [lastuserid] = @uid;  INSERT  INTO dnt_userfields
            ( [uid] ,
              [website] ,
              [icq] ,
              [qq] ,
              [yahoo] ,
              [msn] ,
              [skype] ,
              [location] ,
              [customstatus] ,
              [avatar] ,
              [avatarwidth] ,
              [avatarheight] ,
              [medals] ,
              [bio] ,
              [signature] ,
              [sightml] ,
              [authstr] ,
              [realname] ,
              [idcard] ,
              [mobile] ,
              [phone]
            )
    VALUES  ( @uid ,
              @website ,
              @icq ,
              @qq ,
              @yahoo ,
              @msn ,
              @skype ,
              @location ,
              @customstatus ,
              @avatar ,
              @avatarwidth ,
              @avatarheight ,
              @medals ,
              @bio ,
              @signature ,
              @sightml ,
              @authstr ,
              @realname ,
              @idcard ,
              @mobile ,
              @phone
            )	";
                DbHelper.ExecuteNonQuery(CommandType.Text, sql2, parms);
                UpdateTrendStat(TrendType.Register);
            }
            return uid;
        }

        /// <summary>
        /// �����û���Ϣ.
        /// </summary>
        /// <param name="userinfo">�û���Ϣ</param>
        /// <returns>�����Ƿ�ɹ�</returns>
        public bool UpdateUser(UserInfo userInfo)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Char,20,userInfo.Username),
									   DbHelper.MakeInParam("@nickname",(DbType)NpgsqlDbType.Char,20,userInfo.Nickname),
									   DbHelper.MakeInParam("@password",(DbType)NpgsqlDbType.Char,32,userInfo.Password),
									   DbHelper.MakeInParam("@secques",(DbType)NpgsqlDbType.Char,8,userInfo.Secques),
									   DbHelper.MakeInParam("@spaceid",(DbType)NpgsqlDbType.Integer,4,userInfo.Spaceid),
									   DbHelper.MakeInParam("@gender",(DbType)NpgsqlDbType.Integer,4,userInfo.Gender),
									   DbHelper.MakeInParam("@adminid",(DbType)NpgsqlDbType.Integer,4,userInfo.Adminid),
									   DbHelper.MakeInParam("@groupid",(DbType)NpgsqlDbType.Smallint,2,userInfo.Groupid),
									   DbHelper.MakeInParam("@groupexpiry",(DbType)NpgsqlDbType.Varchar,50,userInfo.Groupexpiry),
									   DbHelper.MakeInParam("@extgroupids",(DbType)NpgsqlDbType.Char,60,userInfo.Extgroupids),
									   DbHelper.MakeInParam("@regip",(DbType)NpgsqlDbType.Char,15,userInfo.Regip),
									   DbHelper.MakeInParam("@joindate",(DbType)NpgsqlDbType.Char,19,userInfo.Joindate),
									   DbHelper.MakeInParam("@lastip",(DbType)NpgsqlDbType.Char,15,userInfo.Lastip),
									   DbHelper.MakeInParam("@lastvisit",(DbType)NpgsqlDbType.Char,19,userInfo.Lastvisit),
									   DbHelper.MakeInParam("@lastactivity",(DbType)NpgsqlDbType.Char,19,userInfo.Lastactivity),
									   DbHelper.MakeInParam("@lastpost",(DbType)NpgsqlDbType.Char,19,userInfo.Lastpost),
									   DbHelper.MakeInParam("@lastpostid",(DbType)NpgsqlDbType.Integer,4,userInfo.Lastpostid),
									   DbHelper.MakeInParam("@lastposttitle",(DbType)NpgsqlDbType.Char,60,userInfo.Lastposttitle),
									   DbHelper.MakeInParam("@posts",(DbType)NpgsqlDbType.Integer,4,userInfo.Posts),
									   DbHelper.MakeInParam("@digestposts",(DbType)NpgsqlDbType.Smallint,2,userInfo.Digestposts),
									   DbHelper.MakeInParam("@oltime",(DbType)NpgsqlDbType.Integer,2,userInfo.Oltime),
									   DbHelper.MakeInParam("@pageviews",(DbType)NpgsqlDbType.Integer,4,userInfo.Pageviews),
									   DbHelper.MakeInParam("@credits",(DbType)NpgsqlDbType.Integer,4,userInfo.Credits),
									   DbHelper.MakeInParam("@extcredits1",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits1),
									   DbHelper.MakeInParam("@extcredits2",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits2),
									   DbHelper.MakeInParam("@extcredits3",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits3),
									   DbHelper.MakeInParam("@extcredits4",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits4),
									   DbHelper.MakeInParam("@extcredits5",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits5),
									   DbHelper.MakeInParam("@extcredits6",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits6),
									   DbHelper.MakeInParam("@extcredits7",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits7),
									   DbHelper.MakeInParam("@extcredits8",(DbType)NpgsqlDbType.Real,8,userInfo.Extcredits8),
									   DbHelper.MakeInParam("@avatarshowid",(DbType)NpgsqlDbType.Integer,4,0),
									   DbHelper.MakeInParam("@email",(DbType)NpgsqlDbType.Char,50,userInfo.Email),
									   DbHelper.MakeInParam("@bday",(DbType)NpgsqlDbType.Char,19,userInfo.Bday),
									   DbHelper.MakeInParam("@sigstatus",(DbType)NpgsqlDbType.Integer,4,userInfo.Sigstatus),  DbHelper.MakeInParam("@salt",(DbType)NpgsqlDbType.Char,4,userInfo.Salt),
									   DbHelper.MakeInParam("@tpp",(DbType)NpgsqlDbType.Integer,4,userInfo.Tpp),
									   DbHelper.MakeInParam("@ppp",(DbType)NpgsqlDbType.Integer,4,userInfo.Ppp),
									   DbHelper.MakeInParam("@templateid",(DbType)NpgsqlDbType.Smallint,2,userInfo.Templateid),
									   DbHelper.MakeInParam("@pmsound",(DbType)NpgsqlDbType.Integer,4,userInfo.Pmsound),
									   DbHelper.MakeInParam("@showemail",(DbType)NpgsqlDbType.Integer,4,userInfo.Showemail),
									   DbHelper.MakeInParam("@newsletter",(DbType)NpgsqlDbType.Integer,4,userInfo.Newsletter),
									   DbHelper.MakeInParam("@invisible",(DbType)NpgsqlDbType.Integer,4,userInfo.Invisible),
									   DbHelper.MakeInParam("@newpm",(DbType)NpgsqlDbType.Integer,4,userInfo.Newpm),
									   DbHelper.MakeInParam("@newpmcount",(DbType)NpgsqlDbType.Integer,4,userInfo.Newpmcount),
									   DbHelper.MakeInParam("@accessmasks",(DbType)NpgsqlDbType.Integer,4,userInfo.Accessmasks),
									   DbHelper.MakeInParam("@onlinestate",(DbType)NpgsqlDbType.Integer,4,userInfo.Onlinestate),
									   DbHelper.MakeInParam("@website",(DbType)NpgsqlDbType.Varchar,80,userInfo.Website),
									   DbHelper.MakeInParam("@icq",(DbType)NpgsqlDbType.Varchar,12,userInfo.Icq),
									   DbHelper.MakeInParam("@qq",(DbType)NpgsqlDbType.Varchar,12,userInfo.Qq),
									   DbHelper.MakeInParam("@yahoo",(DbType)NpgsqlDbType.Varchar,40,userInfo.Yahoo),
									   DbHelper.MakeInParam("@msn",(DbType)NpgsqlDbType.Varchar,40,userInfo.Msn),
									   DbHelper.MakeInParam("@skype",(DbType)NpgsqlDbType.Varchar,40,userInfo.Skype),
									   DbHelper.MakeInParam("@location",(DbType)NpgsqlDbType.Varchar,30,userInfo.Location),
									   DbHelper.MakeInParam("@customstatus",(DbType)NpgsqlDbType.Varchar,30,userInfo.Customstatus),
									   DbHelper.MakeInParam("@avatar",(DbType)NpgsqlDbType.Varchar,255,""),
									   DbHelper.MakeInParam("@avatarwidth",(DbType)NpgsqlDbType.Integer,4,0),
									   DbHelper.MakeInParam("@avatarheight",(DbType)NpgsqlDbType.Integer,4,0),
									   DbHelper.MakeInParam("@medals",(DbType)NpgsqlDbType.Varchar,300, userInfo.Medals),
									   DbHelper.MakeInParam("@bio",(DbType)NpgsqlDbType.Varchar,500,userInfo.Bio),
									   DbHelper.MakeInParam("@signature",(DbType)NpgsqlDbType.Varchar,500,userInfo.Signature),
									   DbHelper.MakeInParam("@sightml",(DbType)NpgsqlDbType.Varchar,1000,userInfo.Sightml),
									   DbHelper.MakeInParam("@authstr",(DbType)NpgsqlDbType.Varchar,20,userInfo.Authstr),
									   DbHelper.MakeInParam("@authtime",(DbType)NpgsqlDbType.Timestamp,4,userInfo.Authtime),
									   DbHelper.MakeInParam("@authflag",(DbType)NpgsqlDbType.Smallint,1,userInfo.Authflag),
                                       DbHelper.MakeInParam("@realname",(DbType)NpgsqlDbType.Varchar,10,userInfo.Realname),
                                       DbHelper.MakeInParam("@idcard",(DbType)NpgsqlDbType.Varchar,20,userInfo.Idcard),
                                       DbHelper.MakeInParam("@mobile",(DbType)NpgsqlDbType.Varchar,20,userInfo.Mobile),
                                       DbHelper.MakeInParam("@phone",(DbType)NpgsqlDbType.Varchar,20,userInfo.Phone),
                                       DbHelper.MakeInParam("@ignorepm",(DbType)NpgsqlDbType.Varchar,1000,userInfo.Ignorepm),
                                       DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,userInfo.Uid)
								   };

            return TypeConverter.ObjectToInt(DbHelper.ExecuteNonQuery(CommandType.StoredProcedure,
                                                                      string.Format("{0}updateuser", BaseConfigs.GetTablePrefix),
                                                                      parms), -1) == 2;
        }

        /// <summary>
        /// ����Ȩ����֤�ַ���
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="authstr">��֤��</param>
        /// <param name="authflag">��֤��־</param>
        public void UpdateAuthStr(int uid, string authStr, int authFlag)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@authstr", (DbType)NpgsqlDbType.Char, 20, authStr),
									   DbHelper.MakeInParam("@authflag", (DbType)NpgsqlDbType.Integer, 4, authFlag) 
								   };
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateuserauthstr", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ����ָ���û��ĸ�������
        /// </summary>
        /// <param name="__userinfo">�û���Ϣ</param>
        /// <returns>����û���������Ϊfalse, ����Ϊtrue</returns>
        public void UpdateUserProfile(UserInfo userInfo)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, userInfo.Uid), 
									   DbHelper.MakeInParam("@nickname",(DbType)NpgsqlDbType.Char,20,userInfo.Nickname),
									   DbHelper.MakeInParam("@gender", (DbType)NpgsqlDbType.Integer, 4, userInfo.Gender), 
									   DbHelper.MakeInParam("@email", (DbType)NpgsqlDbType.Char, 50, userInfo.Email), 
									   DbHelper.MakeInParam("@bday", (DbType)NpgsqlDbType.Char, 10, userInfo.Bday), 
									   DbHelper.MakeInParam("@showemail", (DbType)NpgsqlDbType.Integer, 4, userInfo.Showemail),
									   DbHelper.MakeInParam("@website", (DbType)NpgsqlDbType.Varchar, 80, userInfo.Website), 
									   DbHelper.MakeInParam("@icq", (DbType)NpgsqlDbType.Varchar, 12, userInfo.Icq), 
									   DbHelper.MakeInParam("@qq", (DbType)NpgsqlDbType.Varchar, 12, userInfo.Qq), 
									   DbHelper.MakeInParam("@yahoo", (DbType)NpgsqlDbType.Varchar, 40, userInfo.Yahoo), 
									   DbHelper.MakeInParam("@msn", (DbType)NpgsqlDbType.Varchar, 40, userInfo.Msn), 
									   DbHelper.MakeInParam("@skype", (DbType)NpgsqlDbType.Varchar, 40, userInfo.Skype), 
									   DbHelper.MakeInParam("@location", (DbType)NpgsqlDbType.Varchar, 30, userInfo.Location), 
									   DbHelper.MakeInParam("@bio", (DbType)NpgsqlDbType.Varchar, 500, userInfo.Bio),
                                       DbHelper.MakeInParam("@signature", (DbType)NpgsqlDbType.Varchar, 500, userInfo.Signature),
                                       DbHelper.MakeInParam("@sigstatus", (DbType)NpgsqlDbType.Integer, 4, userInfo.Sigstatus),
                                       DbHelper.MakeInParam("@sightml", (DbType)NpgsqlDbType.Varchar, 1000, userInfo.Sightml),
                                       DbHelper.MakeInParam("@realname",(DbType)NpgsqlDbType.Varchar,10,userInfo.Realname),
                                       DbHelper.MakeInParam("@idcard",(DbType)NpgsqlDbType.Varchar,20,userInfo.Idcard),
                                       DbHelper.MakeInParam("@mobile",(DbType)NpgsqlDbType.Varchar,20,userInfo.Mobile),
                                       DbHelper.MakeInParam("@phone",(DbType)NpgsqlDbType.Varchar,20,userInfo.Phone)
								   };

            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateuserprofile", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �����û���̳����
        /// </summary>
        /// <param name="__userinfo">�û���Ϣ</param>
        /// <returns>����û��������򷵻�false, ���򷵻�true</returns>
        public void UpdateUserForumSetting(UserInfo userInfo)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,userInfo.Uid),
									   DbHelper.MakeInParam("@tpp",(DbType)NpgsqlDbType.Integer,4,userInfo.Tpp),
									   DbHelper.MakeInParam("@ppp",(DbType)NpgsqlDbType.Integer,4,userInfo.Ppp),
									   DbHelper.MakeInParam("@invisible",(DbType)NpgsqlDbType.Integer,4,userInfo.Invisible),
									   DbHelper.MakeInParam("@customstatus",(DbType)NpgsqlDbType.Varchar,30,userInfo.Customstatus)
								   };

            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateuserforumsetting", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �޸��û��Զ�������ֶε�ֵ
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="extid">��չ�ֶ����(1-8)</param>
        /// <param name="pos">���ӵ���ֵ(�����Ǹ���)</param>
        /// <returns>ִ���Ƿ�ɹ�</returns>
        public void UpdateUserExtCredits(int uid, int extId, float pos)
        {
            string commandText = string.Format("UPDATE [{0}users] SET [extcredits{1}]=[extcredits{1}] + {2} WHERE [uid]={3}",
                                                BaseConfigs.GetTablePrefix,
                                                extId,
                                                pos,
                                                uid);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// ���ָ���û���ָ��������չ�ֶε�ֵ
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="extid">��չ�ֶ����(1-8)</param>
        /// <returns>ֵ</returns>
        public float GetUserExtCredits(int uid, int extId)
        {
            string commandText = string.Format("SELECT [extcredits{0}] FROM [{1}users] WHERE [uid]={2}",
                                                extId,
                                                BaseConfigs.GetTablePrefix,
                                                uid);
            return TypeConverter.ObjectToFloat(DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0].Rows[0][0]);
        }

        /// <summary>
        /// �����û�ͷ��
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="avatar">ͷ��</param>
        /// <param name="avatarwidth">ͷ����</param>
        /// <param name="avatarheight">ͷ��߶�</param>
        /// <returns>����û��������򷵻�false, ���򷵻�true</returns>
        public void UpdateUserPreference(int uid, string avatar, int avatarWidth, int avatarHeight, int templateId)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,uid),
									   DbHelper.MakeInParam("@avatar",(DbType)NpgsqlDbType.Varchar,255,avatar),
									   DbHelper.MakeInParam("@avatarwidth",(DbType)NpgsqlDbType.Integer,4,avatarWidth),
									   DbHelper.MakeInParam("@avatarheight",(DbType)NpgsqlDbType.Integer,4,avatarHeight),
                                       DbHelper.MakeInParam("@templateid", (DbType)NpgsqlDbType.Smallint, 4, templateId)
								   };
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateuserpreference", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �����û�����
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="password">����</param>
        /// <param name="originalpassword">�Ƿ��MD5����</param>
        /// <returns>�ɹ�����true����false</returns>
        public void UpdateUserPassword(int uid, string passWord, bool originalPassWord)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@password", (DbType)NpgsqlDbType.Char, 32, originalPassWord ? Utils.MD5(passWord) : passWord)
								   };
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateuserpassword", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// �����û���ȫ����
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="questionid">����id</param>
        /// <param name="answer">��</param>
        /// <returns>�ɹ�����true����false</returns>
        public void UpdateUserSecques(int uid, string secques)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@secques", (DbType)NpgsqlDbType.Char, 8, secques)
								   };
            string commandText = string.Format("UPDATE [{0}users] SET [secques]=@secques WHERE [uid]=@uid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// �����û�����¼ʱ��
        /// </summary>
        /// <param name="uid">�û�id</param>
        public void UpdateUserLastvisit(int uid, string ip)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
									   DbHelper.MakeInParam("@ip", (DbType)NpgsqlDbType.Char,15, ip)
								   };
            string commandText = string.Format("UPDATE [{0}users] SET [lastvisit]=NOW(), [lastip]=@ip WHERE [uid] =@uid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateUserOnlineStateAndLastActivity(string uidList, int onlineState, string activityTime)
        {
            DbParameter[] parms = {
                                        DbHelper.MakeInParam("@onlinestate", (DbType)NpgsqlDbType.Integer, 4, onlineState),
									    DbHelper.MakeInParam("@activitytime", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Parse(activityTime))
								   };
            string commandText = string.Format("UPDATE [{0}users] SET [onlinestate]=@onlinestate,[lastactivity] = @activitytime WHERE [uid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                uidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateUserOnlineStateAndLastActivity(int uid, int onlineState, string activityTime)
        {
            DbParameter[] parms = {
                                        DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
                                        DbHelper.MakeInParam("@onlinestate", (DbType)NpgsqlDbType.Integer, 4, onlineState),
									    DbHelper.MakeInParam("@activitytime", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Parse(activityTime))
								   };
            string commandText = string.Format("UPDATE [{0}users] SET [onlinestate]=@onlinestate,[lastactivity] = @activitytime WHERE [uid]=@uid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateUserOnlineStateAndLastVisit(string uidList, int onlineState, string activityTime)
        {
            DbParameter[] parms = {
                                        DbHelper.MakeInParam("@onlinestate", (DbType)NpgsqlDbType.Integer, 4, onlineState),
									    DbHelper.MakeInParam("@activitytime", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Parse(activityTime))
								   };
            string commandText = string.Format("UPDATE [{0}users] SET [onlinestate]=@onlinestate,[lastvisit] = @activitytime WHERE [uid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                uidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateUserOnlineStateAndLastVisit(int uid, int onlineState, string activityTime)
        {
            DbParameter[] parms = {
                                        DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
                                        DbHelper.MakeInParam("@onlinestate", (DbType)NpgsqlDbType.Integer, 4, onlineState),
									    DbHelper.MakeInParam("@activitytime", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Parse(activityTime))
								   };
            string commandText = string.Format("UPDATE [{0}users] SET [onlinestate]=@onlinestate,[lastvisit] = @activitytime WHERE [uid]=@uid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// �����û���ǰ������ʱ������ʱ��
        /// </summary>
        /// <param name="uid">�û�uid</param>
        public void UpdateUserLastActivity(int uid, string activityTime)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
									   DbHelper.MakeInParam("@activitytime", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Parse(activityTime))
								   };
            string commandText = string.Format("UPDATE [{0}users] SET [lastactivity] = @activitytime  WHERE [uid] = @uid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// �����û���Ϣ����δ������Ϣ������
        /// </summary>
        /// <param name="uid">�û�ID</param>
        /// <param name="pmnum">����Ϣ����</param>
        /// <returns>���¼�¼����</returns>
        public int SetUserNewPMCount(int uid, int pmNum)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@value", (DbType)NpgsqlDbType.Integer, 4, pmNum)
			                      };
            string commandText = string.Format("UPDATE [{0}users] SET [newpmcount]=@value WHERE [uid]=@uid", BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// ����ָ���û���ѫ����Ϣ
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <param name="medals">ѫ����Ϣ</param>
        public void UpdateMedals(int uid, string medals)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@medals", (DbType)NpgsqlDbType.Varchar, 300, medals)
								   };
            string commandText = string.Format("UPDATE [{0}userfields] SET [medals]=@medals WHERE [uid]=@uid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);

        }

        public int DecreaseNewPMCount(int uid, int subVal)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid), 
									   DbHelper.MakeInParam("@subval", (DbType)NpgsqlDbType.Integer, 4, subVal)
			                        };
            try
            {
                string commandText = string.Format("UPDATE [{0}users] SET [newpmcount]=CASE WHEN [newpmcount] >= 0 THEN [newpmcount]-@subval ELSE 0 END WHERE [uid]=@uid", BaseConfigs.GetTablePrefix);
                return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
            }
            catch
            {
                return -1;
            }
        }


        /// <summary>
        /// �����û�������
        /// </summary>
        /// <param name="useridlist">uid�б�</param>
        /// <returns></returns>
        public int UpdateUserDigest(string userIdList)
        {
            string commandText = string.Format("UPDATE [{0}users] SET [digestposts] = (SELECT COUNT([tid]) AS [digest] FROM [{0}topics] WHERE [{0}topics].[posterid] = [{0}users].[uid] AND [digest]>0) WHERE [uid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                userIdList);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// �����û�SpaceID
        /// </summary>
        /// <param name="spaceid">Ҫ���µ�SpaceId</param>
        /// <param name="userId">Ҫ���µ�UserId</param>
        /// <returns></returns>
        public void UpdateUserSpaceId(int spaceId, int userId)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@spaceid",(DbType)NpgsqlDbType.Integer,4,spaceId),
									   DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,userId)
								   };
            string commandText = string.Format("UPDATE [{0}users] SET [spaceid]=@spaceid WHERE [uid]=@uid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public DataTable GetUserIdByAuthStr(string authStr)
        {
            DbParameter[] parms = {
										  DbHelper.MakeInParam("@authstr",(DbType)NpgsqlDbType.Varchar,20,authStr)
				                    };
            string commandText = string.Format("SELECT [uid] FROM [{0}userfields] WHERE DateDiff(d,[authtime],getdate())<=3  AND [authstr]=@authstr",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        }

        /// <summary>
        /// ִ�������û������������ӵĲ�����
        /// </summary>
        /// <param name="onlineuserinfo">�����û���Ϣ����</param>
        /// <param name="timeout">ϵͳ�����û�����ʱ�伴��������</param>
        /// <param name="deletingfrequency">ɾ�������û�Ƶ��(��λ:����)</param>
        /// <returns>��ӳɹ��򷵻ظո���ӵ�olid,ʧ���򷵻�0</returns>
        public int AddOnlineUser(OnlineUserInfo onlineUserInfo, int timeOut, int deletingFrequency)
        {
            //��ʶ��Ҫ�����û�����״̬��0��ʾ��Ҫ����
            int onlinestate = 1;

            // ���timeoutΪ�����������Ҫ��ȷ�����û��Ƿ����ߵ�״̬
            if (timeOut > 0)
            {
                if (onlineUserInfo.Userid > 0)
                    onlinestate = 0;
            }
            else
                timeOut = timeOut * -1;

            if (timeOut > 9999)
                timeOut = 9999;

            DbParameter[] parms = {
									   DbHelper.MakeInParam("@onlinestate",(DbType)NpgsqlDbType.Integer,4,onlinestate),
									   DbHelper.MakeInParam("@userid",(DbType)NpgsqlDbType.Integer,4,onlineUserInfo.Userid),
									   DbHelper.MakeInParam("@ip",(DbType)NpgsqlDbType.Varchar,15,onlineUserInfo.Ip),
									   DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Varchar,40,onlineUserInfo.Username),
									   DbHelper.MakeInParam("@nickname",(DbType)NpgsqlDbType.Varchar,40,onlineUserInfo.Nickname),
									   DbHelper.MakeInParam("@password",(DbType)NpgsqlDbType.Char,32,onlineUserInfo.Password),
									   DbHelper.MakeInParam("@groupid",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Groupid),
									   DbHelper.MakeInParam("@olimg",(DbType)NpgsqlDbType.Varchar,80,onlineUserInfo.Olimg),
									   DbHelper.MakeInParam("@adminid",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Adminid),
									   DbHelper.MakeInParam("@invisible",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Invisible),
									   DbHelper.MakeInParam("@action",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Action),
									   DbHelper.MakeInParam("@lastactivity",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Lastactivity),
									   DbHelper.MakeInParam("@lastposttime",(DbType)NpgsqlDbType.Timestamp,8,TypeConverter.StrToDateTime(onlineUserInfo.Lastposttime)),
									   DbHelper.MakeInParam("@lastpostpmtime",(DbType)NpgsqlDbType.Timestamp,8,TypeConverter.StrToDateTime(onlineUserInfo.Lastpostpmtime)),
									   DbHelper.MakeInParam("@lastsearchtime",(DbType)NpgsqlDbType.Timestamp,8,TypeConverter.StrToDateTime(onlineUserInfo.Lastsearchtime)),
									   DbHelper.MakeInParam("@lastupdatetime",(DbType)NpgsqlDbType.Timestamp,8,TypeConverter.StrToDateTime(onlineUserInfo.Lastupdatetime)),
									   DbHelper.MakeInParam("@forumid",(DbType)NpgsqlDbType.Integer,4,onlineUserInfo.Forumid),
									   DbHelper.MakeInParam("@forumname",(DbType)NpgsqlDbType.Varchar,50,""),
									   DbHelper.MakeInParam("@titleid",(DbType)NpgsqlDbType.Integer,4,onlineUserInfo.Titleid),
									   DbHelper.MakeInParam("@title",(DbType)NpgsqlDbType.Varchar,80,""),
									   DbHelper.MakeInParam("@verifycode",(DbType)NpgsqlDbType.Varchar,10,onlineUserInfo.Verifycode),
									   DbHelper.MakeInParam("@newpms",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Newpms),
									   DbHelper.MakeInParam("@newnotices",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Newnotices)
                                       //DbHelper.MakeInParam("@newfriendrequest",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Newfriendrequest),
                                       //DbHelper.MakeInParam("@newapprequest",(DbType)NpgsqlDbType.Smallint,2,onlineUserInfo.Newapprequest)
								   };

            //�����߱����𵽱�����ݿ�ʱ
            if (EntLibConfigs.GetConfig() != null)
            {
                OnlineTableConnect onlineTableConnect = EntLibConfigs.GetConfig().Onlinetableconnect;
                if (onlineTableConnect.Enable && !string.IsNullOrEmpty(onlineTableConnect.SqlServerConn))
                {
                    if (onlinestate == 0)
                        DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE [{0}users] SET [onlinestate]=1 WHERE [uid]=@userid", BaseConfigs.GetTablePrefix), parms);

                    parms[0].Value = 1;//HACK������Ͳ����ڴ洢���������и���users����Ϣ������ˣ���Ϊ�����ڷ����dnt_online���������ݿ���û��USER�� 
                }
            }
            StringBuilder sb = new StringBuilder();
            if (onlinestate == 0)
                sb.Append("UPDATE [dnt_users] SET [onlinestate]=1 WHERE [uid]=@userid;");
            sb.Append("INSERT INTO [dnt_online] ([userid],[ip],[username],[nickname],[password],[groupid],[olimg],[adminid],[invisible],[action],[lastactivity],[lastposttime],[lastpostpmtime],[lastsearchtime],[lastupdatetime],[forumid],[forumname],[titleid],[title],[verifycode],[newpms],[newnotices])VALUES(@userid,@ip,@username,@nickname,@password,@groupid,@olimg,@adminid,@invisible,@action,@lastactivity,@lastposttime,@lastpostpmtime,@lastsearchtime,@lastupdatetime,@forumid,@forumname,@titleid,@title,@verifycode,@newpms,@newnotices);SELECT currval('dnt_online_olid_seq');");

            int olid = TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text,
                                                                       sb.ToString(),
                                                                        parms));

            //����ϵͳ����Ƶ��(Ĭ��5����)��������û�
            if (_lastRemoveTimeout == 0 || (System.Environment.TickCount - _lastRemoveTimeout) > 60000 * deletingFrequency)
            {
                DeleteExpiredOnlineUsers(timeOut);
                _lastRemoveTimeout = System.Environment.TickCount;
            }
            // ���idֵ̫�����ؽ����߱�
            if (olid > 2147483000)
            {
                CreateOnlineTable();

                olid = TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text,
                                                                     sb.ToString(),
                                                                      parms));

                //DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}createonlineuser", BaseConfigs.GetTablePrefix), parms);
                //  return olid;
            }
            return olid;
        }

        private void DeleteExpiredOnlineUsers(int timeOut)
        {
            System.Text.StringBuilder timeoutStrBuilder = new System.Text.StringBuilder();
            System.Text.StringBuilder memberStrBuilder = new System.Text.StringBuilder();

            DbParameter param = DbHelper.MakeInParam("@expires", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Now.AddMinutes(timeOut * -1));
            IDataReader dr = DbHelper.ExecuteReader(CommandType.Text,
                                                    string.Format("SELECT [olid], [userid] FROM [{0}online] WHERE [lastupdatetime]<@expires", BaseConfigs.GetTablePrefix),
                                                    param);
            while (dr.Read())
            {
                timeoutStrBuilder.Append(",");
                timeoutStrBuilder.Append(dr["olid"].ToString());
                if (dr["userid"].ToString() != "-1")
                {
                    memberStrBuilder.Append(",");
                    memberStrBuilder.Append(dr["userid"].ToString());
                }
            }
            dr.Close();

            if (timeoutStrBuilder.Length > 0)
            {
                timeoutStrBuilder.Remove(0, 1);
                DbHelper.ExecuteNonQuery(CommandType.Text,
                                         string.Format("DELETE FROM [{0}online]  where [olid] IN (" + (timeoutStrBuilder.Length <= 5000 ? timeoutStrBuilder.ToString() : timeoutStrBuilder.ToString().Substring(0, 5000).TrimEnd(',')) + ") ", BaseConfigs.GetTablePrefix));
            }
            if (memberStrBuilder.Length > 0)
            {
                memberStrBuilder.Remove(0, 1);
                DbHelper.ExecuteNonQuery(CommandType.StoredProcedure,
                                         string.Format("UPDATE [{0}users] SET [onlinestate]=0,[lastactivity]=GETDATE() WHERE [uid] IN ({1})", BaseConfigs.GetTablePrefix, (memberStrBuilder.Length <= 5000 ? memberStrBuilder.ToString() : memberStrBuilder.ToString().Substring(0, 5000).TrimEnd(',')))
                                         );
            }
        }

        public DataTable GetUserInfo(int userId)
        {
            string commandText = string.Format("SELECT TOP 1 {0} FROM [{1}users] WHERE [uid]={2}",
                                                DbFields.USERS,
                                                BaseConfigs.GetTablePrefix,
                                                userId);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetUserInfo(string userName, string passWord)
        {
            DbParameter[] parms = {
                                        DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Char, 20, userName),
                                        DbHelper.MakeInParam("@password", (DbType)NpgsqlDbType.Char, 32, passWord)
                                    };
            string commandText = string.Format("SELECT TOP 1 {0} FROM [{1}users] WHERE [username]=@username AND [password]=@password",
                                                DbFields.USERS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        }

        public void UpdateUserSpaceId(int userId)
        {
            string commandText = string.Format("UPDATE [{0}users] SET [spaceid]=ABS([spaceid]) WHERE [uid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                userId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public int GetUserIdByRewriteName(string rewriteName)
        {
            DbParameter parm = DbHelper.MakeInParam("@rewritename", (DbType)NpgsqlDbType.Char, 100, rewriteName);

            string commandText = string.Format("SELECT [userid] FROM [{0}spaceconfigs] WHERE [rewritename]=@rewritename",
                                                BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText, parm), -1);
        }

        public void UpdateUserPMSetting(UserInfo user)
        {
            DbParameter[] parms = {
                                    DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, user.Uid),
                                    DbHelper.MakeInParam("@pmsound", (DbType)NpgsqlDbType.Integer, 4, user.Pmsound),
                                    DbHelper.MakeInParam("@newsletter", (DbType)NpgsqlDbType.Integer, 4, (int)user.Newsletter)
                                  };
            string commandText = string.Format(@"UPDATE [{0}users] SET [pmsound]=@pmsound, [newsletter]=@newsletter WHERE [uid]=@uid",
                                                 BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);

            parms = new DbParameter[] {
                                    DbHelper.MakeInParam("@ignorepm", (DbType)NpgsqlDbType.Varchar, 1000, user.Ignorepm),
                                    DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, user.Uid)
                                     };
            commandText = string.Format(@"UPDATE [{0}userfields] SET [ignorepm]=@ignorepm WHERE [uid]=@uid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void ClearUserSpace(int uid)
        {
            string commandText = string.Format("UPDATE [{0}users] SET [spaceid]=0 WHERE [uid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                uid);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public IDataReader GetUserInfoByName(string userName)
        {
            string commandText = string.Format("SELECT [uid], [username] FROM [{0}users] WHERE [username] LIKE '%{1}%'",
                                                BaseConfigs.GetTablePrefix,
                                                RegEsc(userName));
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }


        public DataTable UserList(int pageSize, int currentPage, string condition)
        {
            int pagetop = (currentPage - 1) * pageSize;
            string commandText;
            if (currentPage == 1)
                commandText = string.Format("SELECT TOP {0} [{1}users].[uid], [{1}users].[username],[{1}users].[nickname], [{1}users].[joindate], [{1}users].[credits], [{1}users].[posts], [{1}users].[lastactivity], [{1}users].[email],[{1}users].[lastvisit],[{1}users].[lastvisit],[{1}users].[accessmasks], [{1}userfields].[location],[{1}usergroups].[grouptitle] FROM [{1}users] LEFT JOIN [{1}userfields] ON [{1}userfields].[uid] = [{1}users].[uid]  LEFT JOIN [{1}usergroups] ON [{1}usergroups].[groupid]=[{1}users].[groupid] WHERE {2} ORDER BY [{1}users].[uid] DESC",
                                             pageSize,
                                             BaseConfigs.GetTablePrefix,
                                             condition);
            else
                commandText = string.Format("SELECT TOP {0} [{1}users].[uid], [{1}users].[username],[{1}users].[nickname], [{1}users].[joindate], [{1}users].[credits], [{1}users].[posts], [{1}users].[lastactivity], [{1}users].[email],[{1}users].[lastvisit],[{1}users].[lastvisit],[{1}users].[accessmasks], [{1}userfields].[location],[{1}usergroups].[grouptitle] FROM [{1}users],[{1}userfields],[{1}usergroups]  WHERE [{1}userfields].[uid] = [{1}users].[uid] AND  [{1}usergroups].[groupid]=[{1}users].[groupid] AND [{1}users].[uid] < (SELECT min([uid])  FROM (SELECT TOP {3} [uid] FROM [{1}users] WHERE {4} ORDER BY [uid] DESC) AS tblTmp ) AND {4} ORDER BY [{1}users].[uid] DESC",
                                             pageSize,
                                             BaseConfigs.GetTablePrefix,
                                             condition,
                                             pagetop,
                                             condition);

            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }

        public void UpdateOnlineTime(int oltimeSpan, int uid)
        {
            DbParameter[] parms = {
                                    DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
                                    DbHelper.MakeInParam("@oltimespan", (DbType)NpgsqlDbType.Smallint, 2, oltimeSpan),
                                    DbHelper.MakeInParam("@lastupdate", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Now),
                                    DbHelper.MakeInParam("@expectedlastupdate", (DbType)NpgsqlDbType.Timestamp, 8, DateTime.Now.AddMinutes(0 - oltimeSpan))
                                };
            string commandText = string.Format("UPDATE [{0}onlinetime] SET [thismonth]=[thismonth]+@oltimespan, [total]=[total]+@oltimespan, [lastupdate]=@lastupdate WHERE [uid]=@uid AND [lastupdate]<=@expectedlastupdate",
                                                BaseConfigs.GetTablePrefix);
            if (DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms) < 1)
            {
                try
                {
                    commandText = string.Format("INSERT INTO [{0}onlinetime]([uid], [thismonth], [total], [lastupdate]) VALUES(@uid, @oltimespan, @oltimespan, @lastupdate)",
                                                 BaseConfigs.GetTablePrefix);
                    DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
                }
                catch { }
            }
        }

        /// <summary>
        /// ����ÿ������ʱ��(����)
        /// </summary>
        public void ResetThismonthOnlineTime()
        {
            DbHelper.ExecuteNonQuery(string.Format("UPDATE [{0}onlinetime] SET [thismonth]=0", BaseConfigs.GetTablePrefix));
        }

        public void SynchronizeOnlineTime(int uid)
        {
            DbParameter[] parms = {
                                    DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
                                  };
            string commandText = string.Format("SELECT [total] FROM [{0}onlinetime] WHERE [uid]=@uid", BaseConfigs.GetTablePrefix);
            int total = TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText, parms));

            commandText = string.Format("UPDATE [{0}users] SET [oltime]={1} WHERE [oltime]<{1} AND [uid]=@uid", BaseConfigs.GetTablePrefix, total);
            if (DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms) < 1)
            {
                try
                {
                    commandText = string.Format("UPDATE [{0}onlinetime] SET [total]=(SELECT [oltime] FROM [{0}users] WHERE [uid]=@uid) WHERE [uid]=@uid",
                                                 BaseConfigs.GetTablePrefix);
                    DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
                }
                catch { }
            }
        }

        public IDataReader GetUserByOnlineTime(string field)
        {
            string commandText = string.Format("SELECT TOP 20 [o].[uid], [u].[username], [o].[{0}] FROM [{1}onlinetime] [o] LEFT JOIN [{1}users] [u] ON [o].[uid]=[u].[uid] ORDER BY [o].[{0}] DESC",
                                                field,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }


        public void UpdateBanUser(int groupId, string groupExpiry, int uid)
        {
            DbParameter[] parms = {
                                    DbHelper.MakeInParam("@groupid", (DbType)NpgsqlDbType.Integer, 4, groupId),
                                    DbHelper.MakeInParam("@groupexpiry", (DbType)NpgsqlDbType.Varchar, 50, groupExpiry),
                                    DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid)
                                };
            //string commandText = string.Format("UPDATE [{0}users] SET [groupid]=@groupid, [groupexpiry]=@groupexpiry WHERE [uid]=@uid",
            //                                    BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateBanUser", BaseConfigs.GetTablePrefix), parms);
        }


        public DataTable SearchSpecialUser(int fid)
        {
            DbParameter[] parms = { 
                                     DbHelper.MakeInParam("@fid",(DbType)NpgsqlDbType.Integer,4,fid)
                                  };
            string commandText = string.Format("SELECT {0} FROM [{1}forums] f JOIN [{1}forumfields] ff ON [f].[fid]=[ff].[fid] where [f].[fid]=@fid",
                                                DbFields.FORUMS_JOIN_FIELDS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        }

        public void UpdateSpecialUser(string permUserList, int fid)
        {
            DbParameter[] parms = { 
                                     DbHelper.MakeInParam("@permuserlist",(DbType)NpgsqlDbType.Text, 0, permUserList),
                                     DbHelper.MakeInParam("@fid",(DbType)NpgsqlDbType.Integer, 4, fid)
                                  };
            string commandText = string.Format("UPDATE [{0}forumfields] SET [permuserlist]=@permuserlist WHERE [fid]=@fid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public int UpdateNewPms(int olid, int count)
        {
            DbParameter[] parms = {
                                     DbHelper.MakeInParam("@action",(DbType)NpgsqlDbType.Char,30,"newpms"),
                                     DbHelper.MakeInParam("@count",(DbType)NpgsqlDbType.Smallint, 2, short.Parse(count.ToString())),
                                     DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer, 4, olid)
                                  };
            //string commandText = string.Format("UPDATE [{0}online] SET [newpms]=@count WHERE [olid]=@olid", BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateonlinenewinfo", BaseConfigs.GetTablePrefix), parms);
            //return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public int UpdateNewNotices(int olId, int plusCount)
        {
            DbParameter[] parms = { 
                                     DbHelper.MakeInParam("@action",(DbType)NpgsqlDbType.Char,30,"newnotice"),
                                     DbHelper.MakeInParam("@count",(DbType)NpgsqlDbType.Smallint, 2, short.Parse(plusCount.ToString())),
                                     DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer, 4, olId)
                                  };
            //string commandText = string.Format("UPDATE [{0}online] SET [newnotices]=[newnotices]+@pluscount WHERE [olid]=@olid",
            //                                    BaseConfigs.GetTablePrefix);
            //return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
            return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateonlinenewinfo", BaseConfigs.GetTablePrefix), parms);
        }

        //public int UpdateNewFriendsRequest(int olId, int count)
        //{
        //    DbParameter[] parms = { 
        //                             DbHelper.MakeInParam("@action",(DbType)NpgsqlDbType.Char,30,"newfriendrequest"),
        //                             DbHelper.MakeInParam("@count",(DbType)NpgsqlDbType.Smallint, 2, short.Parse(count.ToString())),
        //                             DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer, 4, olId)
        //                          };
        //    return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateonlinenewinfo", BaseConfigs.GetTablePrefix), parms);
        //}

        //public int UpdateNewApplicationRequest(int olId, int count)
        //{
        //    DbParameter[] parms = { 
        //                             DbHelper.MakeInParam("@action",(DbType)NpgsqlDbType.Char,30,"newapprequest"),
        //                             DbHelper.MakeInParam("@count",(DbType)NpgsqlDbType.Smallint, 2, short.Parse(count.ToString())),
        //                             DbHelper.MakeInParam("@olid",(DbType)NpgsqlDbType.Integer, 4, olId)
        //                          };
        //    return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updateonlinenewinfo", BaseConfigs.GetTablePrefix), parms);
        //}

        /// <summary>
        /// �õ�ָ���û���ָ��������չ�ֶεĻ���ֵ
        /// </summary>
        /// <returns>��չ��չ����ֵ</returns>
        public int GetUserExtCreditsByUserid(int uid, int extNumber)
        {
            string commandText = string.Format("SELECT TOP 1 [extcredits{0}] FROM [{1}users] WHERE [uid] = {2}",
                                                extNumber,
                                                BaseConfigs.GetTablePrefix,
                                                uid);
            return Convert.ToInt32(TypeConverter.ObjectToFloat(DbHelper.ExecuteScalar(CommandType.Text, commandText), 0));
        }

        /// <summary>
        /// ���������û����ִ洢���̵ķ���
        /// </summary>
        /// <param name="creditExpression">�ܻ��ּ��㹫ʽ</param>
        /// <param name="testCreditExpression">�Ƿ���Ҫ�����ܻ��ּ��㹫ʽ�Ƿ���ȷ</param>
        /// <returns></returns>
        public bool CreateUpdateUserCreditsProcedure(string creditExpression, bool testCreditExpression)
        {
            if (testCreditExpression)
            {
                try
                {
                    DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE [{0}users] SET [credits] = {1} WHERE [uid] = 0",
                        BaseConfigs.GetTablePrefix, creditExpression));
                }
                catch
                {
                    return false;
                }
            }
            string sql = string.Format(@"IF OBJECT_ID('{0}updateusercredits','P') IS NOT NULL
                            DROP PROC [{0}updateusercredits]", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, sql);
            sql = string.Format(@"CREATE PROCEDURE [{0}updateusercredits]
    @uid INT
    AS
    UPDATE [{0}users] SET [credits] = {1} WHERE [uid] = @uid", BaseConfigs.GetTablePrefix, creditExpression);
            DbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return true;
        }

        /// <summary>
        /// ͨ��email��ȡ�û��б�
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public IDataReader GetUserListByEmail(string email)
        {
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("{0}getuserlistbyemail", BaseConfigs.GetTablePrefix), DbHelper.MakeInParam("@email", (DbType)NpgsqlDbType.Varchar, 50, email));
        }

        public DataTable GetUserInfoByEmail(string email)
        {

            return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT [username],[email] FROM [{0}users] WHERE [email]=@email", BaseConfigs.GetTablePrefix), DbHelper.MakeInParam("@email", (DbType)NpgsqlDbType.Varchar, 50, email)).Tables[0];
        }

        //public DataTable GetUserInfoByEmail(string email)
        //{
        //    return DbHelper.ExecuteDataset(CommandType.StoredProcedure, string.Format("SELECT [username],[email] FROM [{0}users] WHERE [email]=@email", BaseConfigs.GetTablePrefix),DbHelper.MakeInParam("@email", (DbType)NpgsqlDbType.Varchar, 50, email)).Tables[0];
        //}

        /// <summary>
        /// ��̳ÿ����Ϣͳ��
        /// </summary>
        /// <param name="trendType">ͳ��������</param>
        public void UpdateTrendStat(TrendType trendType)
        {
            lock (stat)
            {
                stat[trendType]++;
                //�����ϴθ���ʱ��ʱ��������ٴθ���
                if (lastTrendUpdateTime.AddMinutes(5) <= DateTime.Now)
                {
                    DbParameter[] parms = { 
                                     DbHelper.MakeInParam("@daytime",(DbType)NpgsqlDbType.Integer,4,DateTime.Now.ToString("yyyyMMdd")),
                                     DbHelper.MakeInParam("@login",(DbType)NpgsqlDbType.Integer,4, stat[TrendType.Login]),
                                     DbHelper.MakeInParam("@register",(DbType)NpgsqlDbType.Integer,4, stat[TrendType.Register]),
                                     DbHelper.MakeInParam("@topic",(DbType)NpgsqlDbType.Integer,4, stat[TrendType.Topic]),
                                     DbHelper.MakeInParam("@post",(DbType)NpgsqlDbType.Integer,4, stat[TrendType.Post]),
                                     DbHelper.MakeInParam("@poll",(DbType)NpgsqlDbType.Integer,4, stat[TrendType.Poll]),
                                     DbHelper.MakeInParam("@bonus",(DbType)NpgsqlDbType.Integer,4, stat[TrendType.Bonus]),
                                     DbHelper.MakeInParam("@debate",(DbType)NpgsqlDbType.Integer,4, stat[TrendType.Debate])
                                  };
                    string sql = @"SELECT  [daytime]
                        FROM    [dnt_trendstat]
                        WHERE   [daytime] = @daytime";
                    var a = DbHelper.ExecuteScalar(CommandType.Text, sql, parms);
                    if (a == null)
                    {
                        string sql2 = @"  INSERT  INTO [dnt_trendstat]
                    ( [daytime] ,
                      [login] ,
                      [register] ,
                      [topic] ,
                      [post]
                    )
            VALUES  ( @daytime ,
                      0 ,
                      0 ,
                      0 ,
                      0
                    )	;	
		
        UPDATE  [dnt_trendstat]
        SET     [login] = [login] + @login ,
                [register] = [register] + @register ,
                [topic] = [topic] + @topic ,
                [post] = [post] + @post ,
                [poll] = [poll] + @poll ,
                [bonus] = [bonus] + @bonus ,
                [debate] = [debate] + @debate
        WHERE   [daytime] = @daytime";
                        DbHelper.ExecuteNonQuery(CommandType.Text, sql2, parms);

                    }
                    else
                    {
                        string sql2 = @"   	
		
        UPDATE  [dnt_trendstat]
        SET     [login] = [login] + @login ,
                [register] = [register] + @register ,
                [topic] = [topic] + @topic ,
                [post] = [post] + @post ,
                [poll] = [poll] + @poll ,
                [bonus] = [bonus] + @bonus ,
                [debate] = [debate] + @debate
        WHERE   [daytime] = @daytime";
                        DbHelper.ExecuteNonQuery(CommandType.Text, sql2, parms);

                    }


                    Initial();
                    //����ǰʱ������Ϊ������ʱ��
                    lastTrendUpdateTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// �����û�����
        /// </summary>
        /// <param name="credits">����</param>
        /// <param name="startuid">���µ��û�uid��ʼֵ</param>
        public int UpdateUserCredits(string formula, int startuid)
        {
            return DbHelper.ExecuteNonQuery(string.Format("UPDATE [{0}users] SET [credits]={1} WHERE [uid] IN (Select TOP 100 [uid] FROM [{0}users] WHERE [uid] > {2} ORDER BY [uid] ASC)", BaseConfigs.GetTablePrefix, formula, startuid));
        }

    }
}
