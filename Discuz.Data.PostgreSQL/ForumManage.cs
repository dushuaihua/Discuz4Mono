using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Discuz.Data;
using Discuz.Config;
using Discuz.Common;
using Discuz.Entity;
using NpgsqlTypes;
using Npgsql;

namespace Discuz.Data.PostgreSQL
{
    public partial class DataProvider : IDataProvider
    {
        /// <summary>
        /// SQL SERVER SQL���ת��
        /// </summary>
        /// <param name="str">��Ҫת��Ĺؼ��ַ���</param>
        /// <param name="pattern">��Ҫת����ַ�����</param>
        /// <returns>ת�����ַ���</returns>
        private static string RegEsc(string str)
        {
            string[] pattern = { @"%", @"_", @"'" };
            foreach (string s in pattern)
            {
                switch (s)
                {
                    case "%":
                        str = str.Replace(s, "[%]");
                        break;
                    case "_":
                        str = str.Replace(s, "[_]");
                        break;
                    case "'":
                        str = str.Replace(s, "['']");
                        break;
                }
            }
            return str;
        }
        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="displayOrder">��ʾ˳��</param>
        /// <param name="name">����</param>
        /// <param name="url">���ӵ�ַ</param>
        /// <param name="note">��ע</param>
        /// <param name="logo">Logo��ַ</param>
        /// <returns></returns>
        public int AddForumLink(int displayOrder, string name, string url, string note, string logo)
        {
            DbParameter[] parms = {
                                        DbHelper.MakeInParam("@displayorder", (DbType)NpgsqlDbType.Integer, 4, displayOrder),
                                        DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Varchar, 100, name),
                                        DbHelper.MakeInParam("@url", (DbType)NpgsqlDbType.Varchar, 100, url),
                                        DbHelper.MakeInParam("@note", (DbType)NpgsqlDbType.Varchar, 200, note),
                                        DbHelper.MakeInParam("@logo", (DbType)NpgsqlDbType.Varchar, 100, logo)
                                    };
            string commandText = string.Format("INSERT INTO [{0}forumlinks] ([displayorder], [name],[url],[note],[logo]) VALUES (@displayorder,@name,@url,@note,@logo)",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// ���������������
        /// </summary>
        /// <returns></returns>
        public DataTable GetForumLinks()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}forumlinks]", DbFields.FORUM_LINKS, BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }

        /// <summary>
        /// ɾ��ָ����������
        /// </summary>
        /// <param name="forumlinkid"></param>
        /// <returns></returns>
        public int DeleteForumLink(string forumLinkIdList)
        {
            string commandText = string.Format("DELETE FROM [{0}forumlinks] WHERE [id] IN ({1})", BaseConfigs.GetTablePrefix, forumLinkIdList);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        /// <summary>
        /// ����ָ����������
        /// </summary>
        /// <param name="id">��������Id</param>
        /// <param name="displayOrder">��ʾ˳��</param>
        /// <param name="name">����</param>
        /// <param name="url">���ӵ�ַ</param>
        /// <param name="note">��ע</param>
        /// <param name="logo">Logo��ַ</param>
        /// <returns></returns>
        public int UpdateForumLink(int id, int displayOrder, string name, string url, string note, string logo)
        {
            DbParameter[] parms = {
                                        DbHelper.MakeInParam("@id", (DbType)NpgsqlDbType.Integer, 4, id),
                                        DbHelper.MakeInParam("@displayorder", (DbType)NpgsqlDbType.Integer, 4, displayOrder),
                                        DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Varchar, 100, name),
                                        DbHelper.MakeInParam("@url", (DbType)NpgsqlDbType.Varchar, 100, url),
                                        DbHelper.MakeInParam("@note", (DbType)NpgsqlDbType.Varchar, 200, note),
                                        DbHelper.MakeInParam("@logo", (DbType)NpgsqlDbType.Varchar, 100, logo)
                                    };
            string commandText = string.Format("UPDATE [{0}forumlinks] SET [displayorder]=@displayorder,[name]=@name,[url]=@url,[note]=@note,[logo]=@logo WHERE [id]=@id",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }


        /// <summary>
        /// �����ҳ����б�
        /// </summary>
        /// <returns></returns>
        public DataTable GetForumIndexListTable()
        {
            return DbHelper.ExecuteDataset(CommandType.Text, string.Format(@"SELECT CASE WHEN (EXTRACT(EPOCH from (NOW()-lastpost) )/3600)<600 THEN 'new' ELSE 'old' END AS havenew,
dnt_forums.allowbbcode,
dnt_forums.allowblog,
dnt_forums.alloweditrules,
dnt_forums.allowhtml,
dnt_forums.allowimgcode,
dnt_forums.allowpostspecial,
dnt_forums.allowrss,
dnt_forums.allowsmilies,
dnt_forums.allowspecialonly,
dnt_forums.allowtag,
dnt_forums.allowthumbnail,	
dnt_forums.autoclose,	
dnt_forums.colcount,
dnt_forums.curtopics,
dnt_forums.disablewatermark,
dnt_forums.displayorder,
dnt_forums.fid,
dnt_forums.inheritedmod,
dnt_forums.istrade,
dnt_forums.jammer,
dnt_forums.lastpost,
dnt_forums.lastposter,
dnt_forums.lastposterid,
dnt_forums.lasttid,
dnt_forums.lasttitle,
dnt_forums.layer,
dnt_forums.modnewposts,
dnt_forums.name,
dnt_forums.parentid,
dnt_forums.parentidlist,
dnt_forums.pathlist,
dnt_forums.posts,
dnt_forums.recyclebin,
dnt_forums.status,
dnt_forums.subforumcount,
dnt_forums.templateid,
dnt_forums.todayposts,
dnt_forums.topics,
dnt_forumfields.applytopictype,
dnt_forumfields.attachextensions,
dnt_forumfields.description,
dnt_forumfields.fid,
dnt_forumfields.getattachperm,
dnt_forumfields.icon,
dnt_forumfields.moderators,
dnt_forumfields.password,
dnt_forumfields.permuserlist,
dnt_forumfields.postattachperm,
dnt_forumfields.postbytopictype,
dnt_forumfields.postcredits,
dnt_forumfields.postperm,
dnt_forumfields.redirect,
dnt_forumfields.replycredits,
dnt_forumfields.replyperm,
dnt_forumfields.rewritename,
dnt_forumfields.rules,
dnt_forumfields.seodescription,
dnt_forumfields.seokeywords,
dnt_forumfields.topictypeprefix,
dnt_forumfields.topictypes,
dnt_forumfields.viewbytopictype,
dnt_forumfields.viewperm
FROM dnt_forums 
LEFT JOIN dnt_forumfields ON dnt_forums.fid=dnt_forumfields.fid 
WHERE dnt_forums.status > 0 AND layer <= 1 
AND dnt_forums.parentid NOT IN (SELECT fid FROM dnt_forums WHERE status < 1 AND layer = 0) ORDER BY displayorder
", BaseConfigs.GetTablePrefix)).Tables[0];
        }

        /// <summary>
        /// �����ҳ����б�
        /// </summary>
        /// <returns></returns>
        public IDataReader GetForumIndexList()
        {


            return DbHelper.ExecuteReader(CommandType.Text, string.Format(@"SELECT CASE WHEN (EXTRACT(EPOCH from (NOW()-lastpost) )/3600)<600 THEN 'new' ELSE 'old' END AS havenew,
dnt_forums.allowbbcode,
dnt_forums.allowblog,
dnt_forums.alloweditrules,
dnt_forums.allowhtml,
dnt_forums.allowimgcode,
dnt_forums.allowpostspecial,
dnt_forums.allowrss,
dnt_forums.allowsmilies,
dnt_forums.allowspecialonly,
dnt_forums.allowtag,
dnt_forums.allowthumbnail,	
dnt_forums.autoclose,	
dnt_forums.colcount,
dnt_forums.curtopics,
dnt_forums.disablewatermark,
dnt_forums.displayorder,
dnt_forums.fid,
dnt_forums.inheritedmod,
dnt_forums.istrade,
dnt_forums.jammer,
dnt_forums.lastpost,
dnt_forums.lastposter,
dnt_forums.lastposterid,
dnt_forums.lasttid,
dnt_forums.lasttitle,
dnt_forums.layer,
dnt_forums.modnewposts,
dnt_forums.name,
dnt_forums.parentid,
dnt_forums.parentidlist,
dnt_forums.pathlist,
dnt_forums.posts,
dnt_forums.recyclebin,
dnt_forums.status,
dnt_forums.subforumcount,
dnt_forums.templateid,
dnt_forums.todayposts,
dnt_forums.topics,
dnt_forumfields.applytopictype,
dnt_forumfields.attachextensions,
dnt_forumfields.description,
dnt_forumfields.fid,
dnt_forumfields.getattachperm,
dnt_forumfields.icon,
dnt_forumfields.moderators,
dnt_forumfields.password,
dnt_forumfields.permuserlist,
dnt_forumfields.postattachperm,
dnt_forumfields.postbytopictype,
dnt_forumfields.postcredits,
dnt_forumfields.postperm,
dnt_forumfields.redirect,
dnt_forumfields.replycredits,
dnt_forumfields.replyperm,
dnt_forumfields.rewritename,
dnt_forumfields.rules,
dnt_forumfields.seodescription,
dnt_forumfields.seokeywords,
dnt_forumfields.topictypeprefix,
dnt_forumfields.topictypes,
dnt_forumfields.viewbytopictype,
dnt_forumfields.viewperm
FROM dnt_forums 
LEFT JOIN dnt_forumfields ON dnt_forums.fid=dnt_forumfields.fid 
WHERE dnt_forums.status > 0 AND layer <= 1 
AND dnt_forums.parentid NOT IN (SELECT fid FROM dnt_forums WHERE status < 1 AND layer = 0) ORDER BY displayorder
", BaseConfigs.GetTablePrefix));
        }

        /// <summary>
        /// ��ü�����̳��ҳ�б�
        /// </summary>
        /// <returns></returns>
        public DataTable GetArchiverForumIndexList()
        {
            string commandText = string.Format("SELECT [{0}forums].[fid], [{0}forums].[name],[{0}forums].[parentidlist], [{0}forums].[status],[{0}forums].[layer], [{0}forumfields].[viewperm] FROM [{0}forums] LEFT JOIN [{0}forumfields] ON [{0}forums].[fid]=[{0}forumfields].[fid]   ORDER BY [displayorder]",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        private static string GetSubForumSql()
        {
            return string.Format(@"SELECT CASE WHEN Date_part('minute', [lastpost]- NOW())<600 
THEN 'new' ELSE 'old' END AS [havenew],[f].[fid],[f].[parentid],[f].[layer],
[f].[pathlist],[f].[parentidlist],[f].[subforumcount],[f].[name],[f].[status],[f].[colcount],[f].[displayorder],[f].[templateid],[f].[topics],[f].[curtopics],[f].[posts],[f].[todayposts],[f].[lasttid],[f].[lasttitle],[f].[lastpost],[f].[lastposterid],[f].[lastposter],[f].[allowsmilies],[f].[allowrss],[f].[allowhtml],[f].[allowbbcode],[f].[allowimgcode],[f].[allowblog],[f].[istrade],[f].[allowpostspecial],[f].[allowspecialonly],[f].[alloweditrules],[f].[allowthumbnail],[f].[allowtag],[f].[recyclebin],[f].[modnewposts],[f].[jammer],[f].[disablewatermark],[f].[inheritedmod],[f].[autoclose],[ff].[password],[ff].[icon],[ff].[postcredits],[ff].[replycredits],[ff].[redirect],[ff].[attachextensions],[ff].[rules],[ff].[topictypes],[ff].[viewperm],[ff].[postperm],[ff].[replyperm],[ff].[getattachperm],[ff].[postattachperm],[ff].[moderators],[ff].[description],[ff].[applytopictype],[ff].[postbytopictype],[ff].[viewbytopictype],[ff].[topictypeprefix],[ff].[permuserlist],[ff].[seokeywords],[ff].[seodescription],[ff].[rewritename] FROM [dnt_forums] AS [f] LEFT JOIN [dnt_forumfields] AS [ff] ON [f].[fid]=[ff].[fid] 
WHERE [parentid] = @fid AND [status] > 0 ORDER BY [displayorder]",
                                  BaseConfigs.GetTablePrefix, DbFields.FORUMS_JOIN_FIELDS);
        }

        /// <summary>
        /// ����Ӱ���б�
        /// </summary>
        /// <param name="fid">���id</param>
        /// <returns></returns>
        public IDataReader GetSubForumReader(int fid)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer,4, fid)
								   };
            return DbHelper.ExecuteReader(CommandType.Text, GetSubForumSql(), parms);
        }

        /// <summary>
        /// ����Ӱ���б�
        /// </summary>
        /// <param name="fid">���id</param>
        /// <returns></returns>
        public DataTable GetSubForumTable(int fid)
        {
            DbParameter[] parms = {
									   DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer,4, fid)
								   };
            return DbHelper.ExecuteDataset(CommandType.Text, GetSubForumSql(), parms).Tables[0];
        }

        /// <summary>
        /// ���ȫ������б�
        /// </summary>
        /// <returns></returns>
        public DataTable GetForumsTable()
        {
            return DbHelper.ExecuteDataset(CommandType.Text, @"SELECT
[f].[fid],[f].[parentid],[f].[layer],[f].[pathlist],[f].[parentidlist],[f].[subforumcount],[f].[name],[f].[status],[f].[colcount],[f].[displayorder],[f].[templateid],[f].[topics],[f].[curtopics],[f].[posts],[f].[todayposts],[f].[lasttid],[f].[lasttitle],[f].[lastpost],[f].[lastposterid],[f].[lastposter],[f].[allowsmilies],[f].[allowrss],[f].[allowhtml],[f].[allowbbcode],[f].[allowimgcode],[f].[allowblog],[f].[istrade],[f].[allowpostspecial],[f].[allowspecialonly],[f].[alloweditrules],[f].[allowthumbnail],[f].[allowtag],[f].[recyclebin],[f].[modnewposts],[f].[modnewtopics],[f].[jammer],[f].[disablewatermark],[f].[inheritedmod],[f].[autoclose],[ff].[password],[ff].[icon],[ff].[postcredits],[ff].[replycredits],[ff].[redirect],[ff].[attachextensions],[ff].[rules],[ff].[topictypes],[ff].[viewperm],[ff].[postperm],[ff].[replyperm],[ff].[getattachperm],[ff].[postattachperm],[ff].[moderators],[ff].[description],[ff].[applytopictype],[ff].[postbytopictype],[ff].[viewbytopictype],[ff].[topictypeprefix],[ff].[permuserlist],[ff].[seokeywords],[ff].[seodescription],[ff].[rewritename] 
FROM [dnt_forums] AS [f] 
LEFT JOIN [dnt_forumfields] AS [ff] 
ON [f].[fid]=[ff].[fid] 
ORDER BY [f].[displayorder]
").Tables[0];
        }

        /// <summary>
        /// ���õ�ǰ���������(�����Ӱ��)
        /// </summary>
        /// <param name="fid">���id</param>
        /// <returns>������</returns>
        public int SetRealCurrentTopics(int fid)
        {
            DbParameter[] parms = {
                                    DbHelper.MakeInParam("@fid",(DbType)NpgsqlDbType.Integer,4,fid)
                                  };
            return DbHelper.ExecuteNonQuery(CommandType.Text, string.Format(@"UPDATE  [{0}forums]
    SET     [curtopics] = ( SELECT  COUNT(tid)
                            FROM    [dnt_topics]
                            WHERE   [displayorder] >= 0
                                    AND [fid] = @fid
                          )
    WHERE   [fid] = @fid", BaseConfigs.GetTablePrefix), parms);
        }

        public DataTable GetUserGroupsTitle()
        {
            string commandText = string.Format("SELECT [groupid],[grouptitle] FROM [{0}usergroups]  ORDER BY [groupid] ASC",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetUserGroupMaxspacephotosize()
        {
            string commandText = string.Format("SELECT [groupid],[grouptitle],[maxspacephotosize] FROM [{0}usergroups]  ORDER BY [groupid] ASC",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetUserGroupMaxspaceattachsize()
        {
            string commandText = string.Format("SELECT [groupid],[grouptitle],[maxspaceattachsize] FROM [{0}usergroups]  ORDER BY [groupid] ASC",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public int GetForumsMaxDisplayOrder()
        {
            string commandText = string.Format("SELECT MAX(displayorder) FROM [{0}forums]", BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0].Rows[0][0]) + 1;
        }

        public DataTable GetForumsMaxDisplayOrder(int parentid)
        {
            string commandText = string.Format("SELECT MAX([displayorder]) FROM [{0}forums]  WHERE [parentid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                parentid);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public void UpdateForumsDisplayOrder(int minDisplayOrder)
        {
            string commandText = string.Format("UPDATE [{0}forums] SET [displayorder]=[displayorder]+1  WHERE [displayorder]>{1}",
                                                BaseConfigs.GetTablePrefix,
                                                minDisplayOrder);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void UpdateSubForumCount(int fid)
        {
            string commandText = string.Format("UPDATE [{0}forums] SET [subforumcount]=[subforumcount]+1  WHERE [fid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                fid);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public DataTable GetModerators(int fid)
        {
            string commandText = string.Format("SELECT {0} FROM [{1}users] WHERE [uid] IN (SELECT [uid] FROM [{1}moderators] WHERE [inherited]=1 AND [fid]={2})",
                                                DbFields.USERS,
                                                BaseConfigs.GetTablePrefix,
                                                fid);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        public DataTable GetForumField(int fid, string fieldName)
        {
            string commandText = string.Format("SELECT  [{0}] FROM [{1}forumfields] WHERE [fid]={2}  LIMIT 1",
                                                fieldName,
                                                BaseConfigs.GetTablePrefix,
                                                fid);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        public int UpdateForumField(int fid, string fieldName, string fieldValue)
        {
            string commandText = string.Format("UPDATE [{0}forumfields] SET [{1}]='{2}' WHERE [fid]={3}",
                                                BaseConfigs.GetTablePrefix,
                                                fieldName,
                                                fieldValue,
                                                fid);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }
        /// <summary>
        /// ���°�����������
        /// </summary>
        /// <param name="fid">���id</param>
        /// <param name="oldName">�ɰ�������</param>
        /// <param name="newName">�°������֣�Ϊ����ɾ���ð���</param>
        public void UpdateModeratorName(string oldName, string newName)
        {
            DbParameter[] parms = {
                                    DbHelper.MakeInParam("@oldName",(DbType)NpgsqlDbType.Varchar,20,oldName),
                                    DbHelper.MakeInParam("@newName",(DbType)NpgsqlDbType.Varchar,20,newName)
                                  };

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(newName))
            {
                sb.Append(@"UPDATE dnt_forumfields 
	SET MODERATORS=REPLACE(LTRIM(RTRIM(REPLACE(' '+REPLACE(MODERATORS,',',' ')+' ',' '" + oldName + "' ',' '" + newName + "' '))),' ',',')	WHERE instr(','+@oldname+',',','+CONVERT(NVARCHAR(500),MODERATORS)+',') > 0");

            }
            else
            {
                sb.Append(@"UPDATE dnt_forumfields 
	SET MODERATORS = REPLACE(LTRIM(RTRIM(REPLACE(REPLACE(','+CONVERT(NVARCHAR(500),MODERATORS)+',',','" + oldName + "',',','),',',' '))),' ',',')	WHERE instr(','+@oldname+',',','+CONVERT(NVARCHAR(500),MODERATORS)+',') > 0");
            }
            DbHelper.ExecuteNonQuery(CommandType.Text, sb.ToString(), parms);
        }

        public DataRowCollection GetDatechTableIds()
        {
            string commandText = string.Format("SELECT id FROM [{0}tablelist]", BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0].Rows;
        }

        public int UpdateMinMaxField(string postTableName, int postTableId)
        {
            string commandText = string.Format("UPDATE [{0}tablelist] SET [mintid]={1},[maxtid]={2}  WHERE [id]={3}",
                                                BaseConfigs.GetTablePrefix,
                                                GetMinPostTableTid(postTableName),
                                                GetMaxPostTableTid(postTableName),
                                                postTableId);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public int CreateFullTextIndex(string dbName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("USE {0};", dbName);
            sb.Append("execute sp_fulltext_database 'enable';");
            return DbHelper.ExecuteNonQuery(CommandType.Text, sb.ToString());
        }

        public int GetMaxForumId()
        {
            string commandText = string.Format("SELECT COALESCE(MAX(fid), 0) FROM [{0}forums]", BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText));
        }

        /// <summary>
        /// ��ȡ��һ��������Ϣ
        /// </summary>
        /// <returns></returns>
        public int GetFirstFourmID()
        {
            string commantText = string.Format("SELECT   [fid] FROM [{0}forums] WHERE [layer]=1 ORDER BY [fid]  LIMIT 1", BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commantText));
        }

        public DataTable GetShortForumList()
        {
            string commandText = string.Format("SELECT [fid],[name],[parentid] FROM [{0}forums] ORDER BY [displayorder] ASC",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDatasetInMasterDB(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetForumTableBySpecialUser(string userName)
        {
            userName = Utils.ChkSQL(userName);

            string commandText = "";
            if (!Utils.StrIsNullOrEmpty(userName))
                commandText = string.Format("SELECT {0} FROM [{1}forums] f JOIN [{1}forumfields] ff ON [f].[fid]=[ff].[fid] WHERE [ff].[permuserlist] NOT LIKE '' AND [ff].[permuserlist] LIKE '%{2}%'",
                                             DbFields.FORUMS_JOIN_FIELDS,
                                             BaseConfigs.GetTablePrefix,
                                             userName);
            else
                commandText = string.Format("SELECT {0} FROM [{1}forums] f JOIN [{1}forumfields] ff ON [f].[fid]=[ff].[fid] WHERE [ff].[permuserlist] NOT LIKE ''",
                                             DbFields.FORUMS_JOIN_FIELDS,
                                             BaseConfigs.GetTablePrefix);

            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        public DataTable GetForumTableWithSpecialUser(int fid)
        {
            string commandText = string.Format("SELECT {0} FROM [{1}forums] f JOIN [{1}forumfields] ff ON [f].[fid]=[ff].[fid] WHERE [ff].[permuserlist] NOT LIKE '' AND [ff].[fid]={2}",
                                                DbFields.FORUMS_JOIN_FIELDS,
                                                BaseConfigs.GetTablePrefix,
                                                fid);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        public void SaveForumsInfo(ForumInfo forumInfo)
        {
            Npgsql.NpgsqlConnection conn = new NpgsqlConnection(DbHelper.ConnectionString);
            conn.Open();
            using (NpgsqlTransaction trans = conn.BeginTransaction())
            {
                try
                {
                    DbParameter[] parms = {
                        DbHelper.MakeInParam("@parentid", (DbType)NpgsqlDbType.Smallint, 2, forumInfo.Parentid),
				        DbHelper.MakeInParam("@layer", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Layer),
				        DbHelper.MakeInParam("@pathlist", (DbType)NpgsqlDbType.Char, 3000, Utils.StrIsNullOrEmpty(forumInfo.Pathlist) ? " " : forumInfo.Pathlist),
				        DbHelper.MakeInParam("@parentidlist", (DbType)NpgsqlDbType.Char, 300, Utils.StrIsNullOrEmpty(forumInfo.Parentidlist) ? " " : forumInfo.Parentidlist),
				        DbHelper.MakeInParam("@subforumcount", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Subforumcount),
						DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Char, 50, forumInfo.Name),
						DbHelper.MakeInParam("@status", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Status),
						DbHelper.MakeInParam("@colcount", (DbType)NpgsqlDbType.Smallint, 4, forumInfo.Colcount),
						DbHelper.MakeInParam("@displayorder", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Displayorder),
						DbHelper.MakeInParam("@templateid", (DbType)NpgsqlDbType.Smallint, 2, forumInfo.Templateid),
						DbHelper.MakeInParam("@topics", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Topics),
                        DbHelper.MakeInParam("@curtopics", (DbType)NpgsqlDbType.Integer, 4, forumInfo.CurrentTopics),
                        DbHelper.MakeInParam("@posts", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Posts),
                        DbHelper.MakeInParam("@todayposts", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Todayposts),
                        DbHelper.MakeInParam("@lasttid", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Lasttid),
                        DbHelper.MakeInParam("@lasttitle", (DbType)NpgsqlDbType.Char, 60, forumInfo.Lasttitle),
                        DbHelper.MakeInParam("@lastpost", (DbType)NpgsqlDbType.Timestamp, 8,  DateTime.Parse( forumInfo.Lastpost)),
                        DbHelper.MakeInParam("@lastposterid", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Lastposterid),
                        DbHelper.MakeInParam("@lastposter", (DbType)NpgsqlDbType.Char, 20, forumInfo.Lastposter),
                        DbHelper.MakeInParam("@allowsmilies", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowsmilies),
						DbHelper.MakeInParam("@allowrss", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowrss),
						DbHelper.MakeInParam("@allowhtml", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowhtml),
						DbHelper.MakeInParam("@allowbbcode", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowbbcode),
						DbHelper.MakeInParam("@allowimgcode", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowimgcode),
						DbHelper.MakeInParam("@allowblog", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowblog),
                        DbHelper.MakeInParam("@istrade", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Istrade),
                        DbHelper.MakeInParam("@allowpostspecial",(DbType)NpgsqlDbType.Integer,4,forumInfo.Allowpostspecial),
                        DbHelper.MakeInParam("@allowspecialonly",(DbType)NpgsqlDbType.Integer,4,forumInfo.Allowspecialonly),
						DbHelper.MakeInParam("@alloweditrules", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Alloweditrules),
						DbHelper.MakeInParam("@allowthumbnail", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowthumbnail),
                        DbHelper.MakeInParam("@allowtag",(DbType)NpgsqlDbType.Integer,4,forumInfo.Allowtag),
						DbHelper.MakeInParam("@recyclebin", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Recyclebin),
						DbHelper.MakeInParam("@modnewposts", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Modnewposts),
                        DbHelper.MakeInParam("@modnewtopics", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Modnewtopics),
						DbHelper.MakeInParam("@jammer", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Jammer),
						DbHelper.MakeInParam("@disablewatermark", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Disablewatermark),
						DbHelper.MakeInParam("@inheritedmod", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Inheritedmod),
						DbHelper.MakeInParam("@autoclose", (DbType)NpgsqlDbType.Smallint, 2, forumInfo.Autoclose),
						DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Fid),
						DbHelper.MakeInParam("@password", (DbType)NpgsqlDbType.Varchar, 16, forumInfo.Password),
						DbHelper.MakeInParam("@icon", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Icon),
                        DbHelper.MakeInParam("@postcredits", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Postcredits),
                        DbHelper.MakeInParam("@replycredits", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Replycredits),
						DbHelper.MakeInParam("@redirect", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Redirect),
						DbHelper.MakeInParam("@attachextensions", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Attachextensions),
						DbHelper.MakeInParam("@rules", (DbType)NpgsqlDbType.Text, 0, forumInfo.Rules),
						DbHelper.MakeInParam("@topictypes", (DbType)NpgsqlDbType.Text, 0, forumInfo.Topictypes),
						DbHelper.MakeInParam("@viewperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Viewperm),
						DbHelper.MakeInParam("@postperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Postperm),
						DbHelper.MakeInParam("@replyperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Replyperm),
						DbHelper.MakeInParam("@getattachperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Getattachperm),
						DbHelper.MakeInParam("@postattachperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Postattachperm),
                        DbHelper.MakeInParam("@moderators", (DbType)NpgsqlDbType.Text, 0, forumInfo.Moderators),
						DbHelper.MakeInParam("@description", (DbType)NpgsqlDbType.Text, 0, forumInfo.Description),
                        DbHelper.MakeInParam("@applytopictype", (DbType)NpgsqlDbType.Smallint, 1, forumInfo.Applytopictype),
						DbHelper.MakeInParam("@postbytopictype", (DbType)NpgsqlDbType.Smallint, 1, forumInfo.Postbytopictype),
						DbHelper.MakeInParam("@viewbytopictype", (DbType)NpgsqlDbType.Smallint, 1, forumInfo.Viewbytopictype),
						DbHelper.MakeInParam("@topictypeprefix", (DbType)NpgsqlDbType.Smallint, 1, forumInfo.Topictypeprefix),
                        DbHelper.MakeInParam("@permuserlist", (DbType)NpgsqlDbType.Text, 0, forumInfo.Permuserlist),
						DbHelper.MakeInParam("@seokeywords", (DbType)NpgsqlDbType.Varchar, 500, forumInfo.Seokeywords),
                        DbHelper.MakeInParam("@seodescription", (DbType)NpgsqlDbType.Varchar, 500, forumInfo.Seodescription),
                        DbHelper.MakeInParam("@rewritename", (DbType)NpgsqlDbType.Varchar, 20, forumInfo.Rewritename)
					};
                    DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [parentid]=@parentid, [layer]=@layer, [pathlist]=@pathlist, [parentidlist]=@parentidlist, [subforumcount]=@subforumcount, [name]=@name, [status]=@status,[colcount]=@colcount, [displayorder]=@displayorder,[templateid]=@templateid,[topics]=@topics,[curtopics]=@curtopics,[posts]=@posts,[todayposts]=@todayposts,[lasttid]=@lasttid,[lasttitle]=@lasttitle,[lastpost]=@lastpost,[lastposterid]=@lastposterid,[lastposter]=@lastposter,[allowsmilies]=@allowsmilies ,[allowrss]=@allowrss, [allowhtml]=@allowhtml, [allowbbcode]=@allowbbcode, [allowimgcode]=@allowimgcode, [allowblog]=@allowblog,[istrade]=@istrade,[allowpostspecial]=@allowpostspecial,[allowspecialonly]=@allowspecialonly,[alloweditrules]=@alloweditrules ,[allowthumbnail]=@allowthumbnail ,[allowtag]=@allowtag,[recyclebin]=@recyclebin, [modnewposts]=@modnewposts,[modnewtopics]=@modnewtopics,[jammer]=@jammer,[disablewatermark]=@disablewatermark,[inheritedmod]=@inheritedmod,[autoclose]=@autoclose WHERE [fid]=@fid;UPDATE [{0}forumfields] SET [password]=@password,[icon]=@icon,[postcredits]=@postcredits, [replycredits]=@replycredits,[redirect]=@redirect,[attachextensions]=@attachextensions,[rules]=@rules,[topictypes]=@topictypes, [viewperm]=@viewperm,[postperm]=@postperm,[replyperm]=@replyperm,[getattachperm]=@getattachperm,[postattachperm]=@postattachperm, [moderators]=@moderators,[description]=@description,[applytopictype]=@applytopictype,[postbytopictype]=@postbytopictype, [viewbytopictype]=@viewbytopictype,[topictypeprefix]=@topictypeprefix,[permuserlist]=@permuserlist,[seokeywords]=@seokeywords, [seodescription]=@seodescription,[rewritename]=@rewritename  WHERE [fid]=@fid", BaseConfigs.GetTablePrefix), parms);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
            conn.Close();
        }

        public int InsertForumsInf(ForumInfo forumInfo)
        {
            DbParameter[] parms = {
				DbHelper.MakeInParam("@parentid", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Parentid),
				DbHelper.MakeInParam("@layer", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Layer),
				DbHelper.MakeInParam("@pathlist", (DbType)NpgsqlDbType.Char, 3000, Utils.StrIsNullOrEmpty(forumInfo.Pathlist) ? " " : forumInfo.Pathlist),
				DbHelper.MakeInParam("@parentidlist", (DbType)NpgsqlDbType.Char, 300, Utils.StrIsNullOrEmpty(forumInfo.Parentidlist) ? " " : forumInfo.Parentidlist),
				DbHelper.MakeInParam("@subforumcount", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Subforumcount),
				DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Char, 50, forumInfo.Name),
				DbHelper.MakeInParam("@status", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Status),
				DbHelper.MakeInParam("@colcount", (DbType)NpgsqlDbType.Smallint, 4, forumInfo.Colcount),
				DbHelper.MakeInParam("@displayorder", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Displayorder),
				DbHelper.MakeInParam("@templateid", (DbType)NpgsqlDbType.Smallint, 2, forumInfo.Templateid),
				DbHelper.MakeInParam("@allowsmilies", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowsmilies),
				DbHelper.MakeInParam("@allowrss", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowrss),
				DbHelper.MakeInParam("@allowhtml", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowhtml),
				DbHelper.MakeInParam("@allowbbcode", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowbbcode),
				DbHelper.MakeInParam("@allowimgcode", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowimgcode),
				DbHelper.MakeInParam("@allowblog", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowblog),
				DbHelper.MakeInParam("@istrade", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Istrade),
				DbHelper.MakeInParam("@alloweditrules", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Alloweditrules),
				DbHelper.MakeInParam("@allowthumbnail", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Allowthumbnail),
                DbHelper.MakeInParam("@allowtag",(DbType)NpgsqlDbType.Integer,4,forumInfo.Allowtag),
				DbHelper.MakeInParam("@recyclebin", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Recyclebin),
				DbHelper.MakeInParam("@modnewposts", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Modnewposts),
                DbHelper.MakeInParam("@modnewtopics", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Modnewtopics),
				DbHelper.MakeInParam("@jammer", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Jammer),
				DbHelper.MakeInParam("@disablewatermark", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Disablewatermark),
				DbHelper.MakeInParam("@inheritedmod", (DbType)NpgsqlDbType.Integer, 4, forumInfo.Inheritedmod),
				DbHelper.MakeInParam("@autoclose", (DbType)NpgsqlDbType.Smallint, 2, forumInfo.Autoclose),                
                DbHelper.MakeInParam("@allowpostspecial",(DbType)NpgsqlDbType.Integer,4,forumInfo.Allowpostspecial),
                DbHelper.MakeInParam("@allowspecialonly",(DbType)NpgsqlDbType.Integer,4,forumInfo.Allowspecialonly),
				DbHelper.MakeInParam("@description", (DbType)NpgsqlDbType.Text, 0, forumInfo.Description),
				DbHelper.MakeInParam("@password", (DbType)NpgsqlDbType.Varchar, 16, forumInfo.Password),
				DbHelper.MakeInParam("@icon", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Icon),
				DbHelper.MakeInParam("@postcredits", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Postcredits),
				DbHelper.MakeInParam("@replycredits", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Replycredits),
				DbHelper.MakeInParam("@redirect", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Redirect),
				DbHelper.MakeInParam("@attachextensions", (DbType)NpgsqlDbType.Varchar, 255, forumInfo.Attachextensions),
				DbHelper.MakeInParam("@moderators", (DbType)NpgsqlDbType.Text, 0, forumInfo.Moderators),
				DbHelper.MakeInParam("@rules", (DbType)NpgsqlDbType.Text, 0, forumInfo.Rules),
				DbHelper.MakeInParam("@topictypes", (DbType)NpgsqlDbType.Text, 0, forumInfo.Topictypes),
				DbHelper.MakeInParam("@viewperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Viewperm),
				DbHelper.MakeInParam("@postperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Postperm),
				DbHelper.MakeInParam("@replyperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Replyperm),
				DbHelper.MakeInParam("@getattachperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Getattachperm),
				DbHelper.MakeInParam("@postattachperm", (DbType)NpgsqlDbType.Text, 0, forumInfo.Postattachperm),
				DbHelper.MakeInParam("@seokeywords", (DbType)NpgsqlDbType.Varchar, 500, forumInfo.Seokeywords),
                DbHelper.MakeInParam("@seodescription", (DbType)NpgsqlDbType.Varchar, 500, forumInfo.Seodescription),
                DbHelper.MakeInParam("@rewritename", (DbType)NpgsqlDbType.Varchar, 20, forumInfo.Rewritename)
			};


            DbHelper.ExecuteDataset(CommandType.Text, string.Format("INSERT INTO [{0}forums] ([parentid],[layer],[pathlist],[parentidlist],[subforumcount],[name],[status],[colcount],[displayorder],[templateid],[allowsmilies],[allowrss],[allowhtml],[allowbbcode],[allowimgcode],[allowblog],[istrade],[alloweditrules],[recyclebin],[modnewposts],[modnewtopics],[jammer],[disablewatermark],[inheritedmod],[autoclose],[allowthumbnail],[allowtag],[allowpostspecial],[allowspecialonly]) VALUES (@parentid,@layer,@pathlist,@parentidlist,@subforumcount,@name,@status, @colcount, @displayorder,@templateid,@allowsmilies,@allowrss,@allowhtml,@allowbbcode,@allowimgcode,@allowblog,@istrade,@alloweditrules,@recyclebin,@modnewposts,@modnewtopics,@jammer,@disablewatermark,@inheritedmod,@autoclose,@allowthumbnail,@allowtag,@allowpostspecial,@allowspecialonly); INSERT INTO [{0}forumfields] ([fid],[description],[password],[icon],[postcredits],[replycredits],[redirect],[attachextensions],[moderators],[rules],[topictypes],[viewperm],[postperm],[replyperm],[getattachperm],[postattachperm],[seokeywords],[seodescription],[rewritename]) VALUES ((SELECT COALESCE(MAX(fid), 0) FROM [dnt_forums]),@description,@password,@icon,@postcredits,@replycredits,@redirect,@attachextensions,@moderators,@rules,@topictypes,@viewperm,@postperm,@replyperm,@getattachperm,@postattachperm,@seokeywords,@seodescription,@rewritename) ", BaseConfigs.GetTablePrefix), parms);
            return GetMaxForumId();
        }



        public void MovingForumsPos(string currentFid, string targetFid, bool isAsChildNode, string extName)
        {
            Npgsql.NpgsqlConnection conn = new NpgsqlConnection(DbHelper.ConnectionString);
            conn.Open();

            using (NpgsqlTransaction trans = conn.BeginTransaction())
            {
                try
                {
                    //ȡ�õ�ǰ��̳������Ϣ
                    DataRow dr = DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT   {0} FROM [{1}forums] WHERE [fid] = {2}  LIMIT 1", DbFields.FORUMS, BaseConfigs.GetTablePrefix, currentFid)).Tables[0].Rows[0];

                    //ȡ��Ŀ����̳������Ϣ
                    DataRow targetdr = DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT      FROM [{1}forums] WHERE [fid]={2} LIMIT 1", DbFields.FORUMS, BaseConfigs.GetTablePrefix, targetFid)).Tables[0].Rows[0];

                    //��ǰ��̳�����Ӱ��ʱ
                    if (DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT   FID FROM [{0}forums] WHERE [parentid]={1} LIMIT 1", BaseConfigs.GetTablePrefix, currentFid)).Tables[0].Rows.Count > 0)
                    {
                        #region

                        string commandText = "";
                        if (isAsChildNode) //��Ϊ��̳�Ӱ�����
                        {
                            //��λ�ڵ�ǰ��̳���(����)��ʾ˳��֮�����̳���ȫ����1(Ϊ�¼������̳�����λ���)
                            commandText = string.Format("UPDATE [{0}forums] SET [displayorder]=[displayorder]+1 WHERE [displayorder]>={1}", BaseConfigs.GetTablePrefix, TypeConverter.ObjectToInt(targetdr["displayorder"]) + 1);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);

                            //���µ�ǰ��̳���������Ϣ
                            commandText = string.Format("UPDATE [{0}forums] SET [parentid]='{1}',[layer]={2},[pathlist]='{3}'+[pathlist],[parentidlist]=(CASE WHEN [parentidlist]='0' THEN '{4}' ELSE '{4},'+[parentidlist] END),[displayorder]='{5}' WHERE [fid]={6}",
                                BaseConfigs.GetTablePrefix, targetdr["fid"].ToString(), TypeConverter.ObjectToInt(targetdr["layer"]) + 1,
                                targetdr["pathlist"].ToString().Trim(), (targetdr["parentidlist"].ToString().Trim() == "0" ? "" : targetdr["parentidlist"].ToString().Trim() + ",") + targetdr["fid"],
                                TypeConverter.ObjectToInt(targetdr["displayorder"]) + 1, currentFid);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);
                            DataRow afterUpdatedr = DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT  {0} FROM [{1}forums] WHERE [fid] = {2}  LIMIT 1", DbFields.FORUMS, BaseConfigs.GetTablePrefix, currentFid)).Tables[0].Rows[0];
                            commandText = string.Format("UPDATE [{0}forums] SET [layer]={1},[pathlist]='{2}'+[pathlist],[parentidlist]='{3},'+[parentidlist] WHERE [parentid]={4}",
                                BaseConfigs.GetTablePrefix, TypeConverter.ObjectToInt(afterUpdatedr["layer"]) + 1,
                                afterUpdatedr["pathlist"].ToString().Trim(), afterUpdatedr["parentidlist"].ToString().Trim(), currentFid);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);
                        }
                        else //��Ϊͬ����̳���,��Ŀ����̳���֮ǰ����
                        {
                            //��λ�ڰ�����ǰ��̳�����ʾ˳��֮�����̳���ȫ����1(Ϊ�¼������̳�����λ���)
                            commandText = string.Format("UPDATE [{0}forums] SET [displayorder]=[displayorder]+1 WHERE [displayorder]>={1} OR [fid]={2}", BaseConfigs.GetTablePrefix, targetdr["displayorder"], targetdr["fid"]);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);

                            //���µ�ǰ��̳���������Ϣ
                            commandText = string.Format("UPDATE [{0}forums] SET [parentid]='{1}',[layer]={2},[parentidlist]={3},[displayorder]='{4}' WHERE [fid]={5}",
                                BaseConfigs.GetTablePrefix, targetdr["parentid"], targetdr["layer"], targetdr["parentidlist"], targetdr["displayorder"], currentFid);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);
                            DataRow afterUpdatedr = DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT   {0} FROM [{1}forums] WHERE [fid] = {2} LIMIT 1", DbFields.FORUMS, BaseConfigs.GetTablePrefix, currentFid)).Tables[0].Rows[0];
                            commandText = string.Format("UPDATE [{0}forums] SET [layer]={1},[parentidlist]='{2}' WHERE [parentid]={3}",
                                BaseConfigs.GetTablePrefix, TypeConverter.ObjectToInt(afterUpdatedr["layer"]) + 1, (afterUpdatedr["parentidlist"].ToString().Trim() == "0" ? "" : afterUpdatedr["parentidlist"].ToString().Trim() + ",") + afterUpdatedr["fid"], currentFid);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);
                        }

                        //������������������Ӱ��İ������������
                        if (dr["topics"].ToString() != "0" && TypeConverter.ObjectToInt(dr["topics"]) > 0 && dr["posts"].ToString() != "0" && TypeConverter.ObjectToInt(dr["posts"]) > 0)
                        {
                            if (!Utils.StrIsNullOrEmpty(dr["parentidlist"].ToString()))
                                DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [topics]=[topics]-{1},[posts]=[posts]-{2}  WHERE [fid] IN ({3})", BaseConfigs.GetTablePrefix, dr["topics"], dr["posts"], dr["parentidlist"].ToString().Trim()));

                            if (!Utils.StrIsNullOrEmpty(targetdr["parentidlist"].ToString()))
                                DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [topics]=[topics]+{1},[posts]=[posts]+{2}  WHERE [fid] IN ({3})", BaseConfigs.GetTablePrefix, dr["topics"], dr["posts"], targetdr["parentidlist"].ToString().Trim()));
                        }

                        #endregion
                    }
                    else //��ǰ��̳��鲻���Ӱ�
                    {
                        #region

                        //���þɵĸ�һ��������̳��
                        DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [subforumcount]=[subforumcount]-1 WHERE [fid]={1}", BaseConfigs.GetTablePrefix, dr["parentid"]));

                        //��λ�ڵ�ǰ�ڵ���ʾ˳��֮��Ľڵ�ȫ����1 [��ɾ���ڵ��Ч��]
                        if (isAsChildNode) //��Ϊ����̳������
                        {
                            //������Ӧ�ı�Ӱ��İ������������
                            if (dr["topics"].ToString() != "0" && TypeConverter.ObjectToInt(dr["topics"].ToString()) > 0 && dr["posts"].ToString() != "0" && TypeConverter.ObjectToInt(dr["posts"]) > 0)
                            {
                                DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [topics]=[topics]-{1},[posts]=[posts]-{2} WHERE [fid] IN ({3})", BaseConfigs.GetTablePrefix, dr["topics"], dr["posts"], dr["parentidlist"]));
                                if (targetdr["parentidlist"].ToString().Trim() != "")
                                    DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [topics]=[topics]+{1},[posts]=[posts]+{2} WHERE [fid] IN ({3},{4})", BaseConfigs.GetTablePrefix, dr["topics"], dr["posts"], targetdr["parentidlist"], targetFid));
                            }

                            //��λ�ڵ�ǰ��̳�����ʾ˳��֮�����̳���ȫ����1(Ϊ�¼������̳�����λ���)
                            string commandText = string.Format(string.Format("UPDATE [{0}forums] SET [displayorder]=[displayorder]+1 WHERE [displayorder]>={1}", BaseConfigs.GetTablePrefix, TypeConverter.ObjectToInt(targetdr["displayorder"]) + 1));
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);

                            //�����µĸ�һ��������̳��
                            DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [subforumcount]=[subforumcount]+1 WHERE [fid]={1}", BaseConfigs.GetTablePrefix, targetFid));

                            string parentidlist = null;
                            if (targetdr["parentidlist"].ToString().Trim() == "0")
                                parentidlist = targetFid;
                            else
                                parentidlist = targetdr["parentidlist"].ToString().Trim() + "," + targetFid;

                            //���µ�ǰ��̳���������Ϣ
                            commandText = string.Format("UPDATE [{0}forums] SET [parentid]='{1}',[layer]='{2}',[pathlist]='{3}', [parentidlist]='{4}',[displayorder]='{5}' WHERE [fid]={6}",
                                                      BaseConfigs.GetTablePrefix,
                                                      targetdr["fid"].ToString(),
                                                      TypeConverter.ObjectToInt(targetdr["layer"]) + 1,
                                                      targetdr["pathlist"].ToString().Trim() + "<a href=\"showforum-" + currentFid + extName + "\">" + dr["name"].ToString().Trim().Replace("'", "''") + "</a>",
                                                      parentidlist,
                                                      TypeConverter.ObjectToInt(targetdr["displayorder"]) + 1,
                                                      currentFid);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);

                        }
                        else //��Ϊͬ����̳���,��Ŀ����̳���֮ǰ����
                        {
                            //������Ӧ�ı�Ӱ��İ������������
                            if (dr["topics"].ToString() != "0" && TypeConverter.ObjectToInt(dr["topics"]) > 0 && dr["posts"].ToString() != "0" && TypeConverter.ObjectToInt(dr["posts"]) > 0)
                            {
                                DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [topics]=[topics]-{1},[posts]=[posts]-{2}  WHERE [fid] IN ({3})", BaseConfigs.GetTablePrefix, dr["topics"], dr["posts"], dr["parentidlist"]));
                                DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [topics]=[topics]+{1},[posts]=[posts]+{2}  WHERE [fid] IN ({3})", BaseConfigs.GetTablePrefix, dr["topics"], dr["posts"], targetdr["parentidlist"]));
                            }

                            //��λ�ڰ�����ǰ��̳�����ʾ˳��֮�����̳���ȫ����1(Ϊ�¼������̳�����λ���)
                            string commandText = string.Format("UPDATE [{0}forums] SET [displayorder]=[displayorder]+1 WHERE [displayorder]>={1} OR [fid]={2}",
                                                                BaseConfigs.GetTablePrefix,
                                                                TypeConverter.ObjectToInt(targetdr["displayorder"]) + 1,
                                                                targetdr["fid"]);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);

                            //�����µĸ�һ��������̳��
                            DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("UPDATE [{0}forums]  SET [subforumcount]=[subforumcount]+1 WHERE [fid]={1}", BaseConfigs.GetTablePrefix, targetdr["parentid"]));
                            string parentpathlist = "";
                            DataTable dt = DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT   [pathlist] FROM [{0}forums] WHERE [fid]={1} LIMIT 1", BaseConfigs.GetTablePrefix, targetdr["parentid"])).Tables[0];
                            if (dt.Rows.Count > 0)
                                parentpathlist = DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT   [pathlist] FROM [{0}forums] WHERE [fid]={1} LIMIT 1", BaseConfigs.GetTablePrefix, targetdr["parentid"])).Tables[0].Rows[0][0].ToString().Trim();

                            //���µ�ǰ��̳���������Ϣ
                            commandText = string.Format("UPDATE [{0}forums]  SET [parentid]='{1}',[layer]='{2}',[pathlist]='{3}', [parentidlist]='{4}',[displayorder]='{5}' WHERE [fid]={6}",
                                                      BaseConfigs.GetTablePrefix,
                                                      targetdr["parentid"],
                                                      targetdr["layer"],
                                                      parentpathlist + "<a href=\"showforum-" + currentFid + extName + "\">" + dr["name"].ToString().Trim() + "</a>",
                                                      targetdr["parentidlist"].ToString().Trim(),
                                                      TypeConverter.ObjectToInt(targetdr["displayorder"]),
                                                      currentFid);
                            DbHelper.ExecuteDataset(trans, CommandType.Text, commandText);
                        }

                        #endregion
                    }
                    trans.Commit();
                }

                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
                conn.Close();
            }
        }

        public bool IsExistSubForum(int fid)
        {
            DbParameter parm = DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, fid);
            return Convert.ToInt32(DbHelper.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(fid) FROM [{0}forums] WHERE [parentid]=@fid", BaseConfigs.GetTablePrefix), parm)) > 0;
        }

        public void DeleteForumsByFid(string postName, string fid)
        {
            int forumsID = Convert.ToInt32(fid);
            Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(DbHelper.ConnectionString);
            conn.Open();
            using (NpgsqlTransaction trans = conn.BeginTransaction())
            {
                try
                {
                    DbParameter[] parms ={
                                             DbHelper.MakeInParam("@fid",(DbType)NpgsqlDbType.Integer,4,forumsID),
                                             DbHelper.MakeInParam("@postname",(DbType)NpgsqlDbType.Varchar,50,postName),DbHelper.MakeInParam("@parentid",(DbType)NpgsqlDbType.Integer,50,0),DbHelper.MakeInParam("@displayorder",(DbType)NpgsqlDbType.Integer,50,0)
                                         };
                    IDataReader dr = DbHelper.ExecuteReader(CommandType.Text, "SELECT  parentid,[displayorder] FROM [dnt_forums] WHERE [fid]=@fid  LIMIT 1", parms);
                    int parentid = 0;
                    int displayorder = 0;
                    if (dr.Read())
                    {
                        parentid = (int)dr["parentid"];
                        displayorder = (int)dr["displayorder"];
                        dr.Close();
                        parms[2].Value = parentid;
                        parms[3].Value = displayorder;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(@"UPDATE [dnt_forums] SET [displayorder]=[displayorder]-1 WHERE [displayorder]>@displayorder;
UPDATE [dnt_forums] SET [subforumcount]=[subforumcount]-1 WHERE  [fid]=@parentid;
DELETE FROM [dnt_forumfields] WHERE [fid]=@fid;
DELETE FROM [dnt_polls] WHERE [tid] IN (SELECT [tid] FROM [dnt_topics] WHERE [fid]=@fid);");

                        sb.AppendFormat("DELETE FROM [dnt_attachments] WHERE [tid] IN(SELECT [tid] FROM [dnt_topics] WHERE [fid]={0}) OR [pid] IN(SELECT [pid] FROM [{1}] WHERE [fid]={0}) ;", fid, postName);

                        sb.AppendFormat("DELETE FROM [{0}] WHERE [fid]={1};", postName, fid);
                        sb.Append(@"DELETE FROM [dnt_topics] WHERE [fid]=@fid;
DELETE FROM [dnt_forums] WHERE  [fid]=@fid;
DELETE FROM [dnt_moderators] WHERE  [fid]=@fid;");
                        DbHelper.ExecuteNonQuery(trans, CommandType.Text, sb.ToString(), parms);
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
            }
            conn.Close();
        }

        public DataTable GetParentIdByFid(int fid)
        {
            DbParameter parm = DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, fid);
            return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT [parentid] From [{0}forums] WHERE [inheritedmod]=1 AND [fid]=@fid", BaseConfigs.GetTablePrefix), parm).Tables[0];
        }

        public void InsertForumsModerators(string fid, string moderators, int displayOrder, int inherited)
        {
            //NpgsqlConnection conn = new NpgsqlConnection(DbHelper.ConnectionString);
            //conn.Open();
            //using (NpgsqlTransaction trans = conn.BeginTransaction())
            //{
            //    try
            //    {
            //        DbParameter[] parms = {
            //                              DbHelper.MakeInParam("@displayorder",(DbType)NpgsqlDbType.Integer,4,displayOrder),
            //                              DbHelper.MakeInParam("@moderators",(DbType)NpgsqlDbType.Varchar,500,moderators),
            //                              DbHelper.MakeInParam("@fid",(DbType)NpgsqlDbType.Integer,4,fid),
            //                              DbHelper.MakeInParam("@inherited",(DbType)NpgsqlDbType.Integer,4,inherited),
            //                              };
            //        DbHelper.ExecuteNonQuery(trans, CommandType.StoredProcedure, string.Format("[{0}updateforumsmoderators]", BaseConfigs.GetTablePrefix), parms);
            //        trans.Commit();
            //    }
            //    catch (Exception ex)
            //    {
            //        trans.Rollback();
            //        throw ex;
            //    }
            //}
            //conn.Close();
            int count = displayOrder;


            //���ݿ��д��ڵ��û�
            string usernamelist = "";
            //���������̳�İ�������
            foreach (string username in moderators.Split(','))
            {
                if (username.Trim() != "")
                {
                    DbParameter[] prams =
								{
									DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Varchar, 20, username.Trim())
								};
                    //��ȡ����ǰ�ڵ����Ϣ
                    DataTable dt = DbHelper.ExecuteDataset(CommandType.Text, "Select [uid] From [" + BaseConfigs.GetTablePrefix + "users] Where [groupid]<>7 AND [groupid]<>8 AND [username]=@username LIMIT 1", prams).Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO [" + BaseConfigs.GetTablePrefix + "moderators] (uid,fid,displayorder,inherited) VALUES(" + dt.Rows[0][0].ToString() + "," + fid + "," + count.ToString() + "," + inherited.ToString() + ")");
                        usernamelist = usernamelist + username.Trim() + ",";
                        count++;
                    }
                }
            }

            if (usernamelist != "")
            {
                DbParameter[] prams1 =
							{
								DbHelper.MakeInParam("@moderators", (DbType)NpgsqlDbType.Varchar, 255, usernamelist.Substring(0, usernamelist.Length - 1))

							};
                DbHelper.ExecuteNonQuery(CommandType.Text, "Update [" + BaseConfigs.GetTablePrefix + "forumfields] SET [moderators]=@moderators  WHERE [fid] =" + fid, prams1);
            }
            else
            {
                DbHelper.ExecuteNonQuery(CommandType.Text, "Update [" + BaseConfigs.GetTablePrefix + "forumfields] SET [moderators]='' WHERE [fid] =" + fid);
            }

        }



        public void CombinationForums(string sourceFid, string targetFid, string fidList)
        {
            SqlConnection conn = new SqlConnection(DbHelper.ConnectionString);
            conn.Open();
            using (SqlTransaction trans = conn.BeginTransaction())
            {
                try
                {
                    //�����������������Ϣ
                    DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}topics] SET [fid]={1}  WHERE [fid]={2}",
                                                                                     BaseConfigs.GetTablePrefix,
                                                                                     targetFid,
                                                                                     sourceFid));
                    //Ҫ����Ŀ����̳��������
                    int totaltopics = TypeConverter.ObjectToInt(DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT COUNT(tid)  FROM [{0}topics] WHERE [fid] IN ({1})", BaseConfigs.GetTablePrefix, fidList)).Tables[0].Rows[0][0]);

                    int totalposts = 0;
                    foreach (DataRow postdr in DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT [id] From [{0}tablelist]", BaseConfigs.GetTablePrefix)).Tables[0].Rows)
                    {
                        DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}posts{1}] SET [fid]={2}  WHERE [fid]={3}", BaseConfigs.GetTablePrefix, postdr["id"], targetFid, sourceFid));

                        //Ҫ����Ŀ����̳��������
                        totalposts = totalposts + TypeConverter.ObjectToInt(DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT COUNT(pid)  FROM [{0}posts{1}] WHERE [fid] IN({2})", BaseConfigs.GetTablePrefix, postdr["id"], fidList)).Tables[0].Rows[0][0]);
                    }

                    DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [topics]={1} ,[posts]={2}  WHERE [fid]={3}", BaseConfigs.GetTablePrefix, totaltopics, totalposts, targetFid));

                    //��ȡԴ��̳��Ϣ
                    DataRow dr = DbHelper.ExecuteDataset(trans, CommandType.Text, string.Format("SELECT  {0} FROM [{1}forums] WHERE [fid]={2}  LIMIT 1", DbFields.FORUMS, BaseConfigs.GetTablePrefix, sourceFid)).Tables[0].Rows[0];

                    //�����ڵ�ǰ�ڵ�����λ��֮��Ľڵ�,����1����
                    DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [displayorder]=[displayorder]-1 WHERE [displayorder]>{1}", BaseConfigs.GetTablePrefix, dr["displayorder"]));

                    //�޸ĸ�����е�����̳����
                    DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("UPDATE [{0}forums] SET [subforumcount]=[subforumcount]-1 WHERE [fid]={1}", BaseConfigs.GetTablePrefix, dr["parentid"]));

                    //ɾ����ǰ�ڵ�ĸ߼����Բ���
                    DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("DELETE FROM [{0}forumfields] WHERE [fid]={1}", BaseConfigs.GetTablePrefix, sourceFid));

                    //ɾ��Դ��̳���
                    DbHelper.ExecuteNonQuery(trans, CommandType.Text, string.Format("DELETE FROM [{0}forums] WHERE [fid]={1}", BaseConfigs.GetTablePrefix, sourceFid));
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
            conn.Close();
        }

        public void UpdateSubForumCount(int subForumCount, int fid)
        {
            DbParameter[] parms =
			{
                DbHelper.MakeInParam("@subforumcount", (DbType)NpgsqlDbType.Integer, 4, subForumCount),
                DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, fid)
			};
            string commandText = string.Format("UPDATE [{0}forums] SET [subforumcount]=@subforumcount WHERE [fid]=@fid", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteDataset(CommandType.Text, commandText, parms);
        }


        public DataTable GetMainForum()
        {
            return DbHelper.ExecuteDataset(CommandType.Text, string.Format("SELECT [fid],[parentid],[layer],[pathlist],[parentidlist],[subforumcount],[name],[status],[colcount],[displayorder],[templateid],[topics],[curtopics],[posts],[todayposts],[lasttid],[lasttitle],[lastpost],[lastposterid],[lastposter],[allowsmilies],[allowrss],[allowhtml],[allowbbcode],[allowimgcode],[allowblog],[istrade],[allowpostspecial],[allowspecialonly],[alloweditrules],[allowthumbnail],[allowtag],[recyclebin],[modnewposts],[jammer],[disablewatermark],[inheritedmod],[autoclose] FROM [{0}forums]  WHERE [layer]=0  Order By [displayorder] ASC", BaseConfigs.GetTablePrefix)).Tables[0];
        }

        public void SetStatusInForum(int status, int fid)
        {
            DbParameter[] parms =
			{
                DbHelper.MakeInParam("@status", (DbType)NpgsqlDbType.Integer, 4, status),
                DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, fid)
			};
            DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE [{0}forums] SET [status]=@status WHERE [fid]=@fid", BaseConfigs.GetTablePrefix), parms);
        }

        public DataTable GetForumByParentid(int parentid)
        {
            DbParameter[] parms =
			{
                DbHelper.MakeInParam("@parentid", (DbType)NpgsqlDbType.Integer, 4, parentid)
			};
            string commandText = string.Format("SELECT {0} FROM [{1}forums] WHERE [parentid]=@parentid ORDER BY [DisplayOrder]",
                                                DbFields.FORUMS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText, parms).Tables[0];
        }

        public void UpdateStatusByFidlist(string fidList)
        {
            string commandText = string.Format("UPDATE [{0}forums] SET [status]=0 WHERE [fid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                fidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void UpdateStatusByFidlistOther(string fidList)
        {
            string commandText = string.Format("UPDATE [{0}forums] SET [status]=1 WHERE [status]>1 AND [fid] IN ({1})", BaseConfigs.GetTablePrefix, fidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public bool BatchSetForumInf(ForumInfo forumInfo, BatchSetParams bsp, string fidList)
        {
            StringBuilder forums = new StringBuilder();
            StringBuilder forumfields = new StringBuilder();

            forums.AppendFormat("UPDATE [{0}forums] SET ", BaseConfigs.GetTablePrefix);
            if (bsp.SetSetting)
            {
                forums.AppendFormat("[Allowsmilies]='{0}' ,", forumInfo.Allowsmilies);
                forums.AppendFormat("[Allowrss]='{0}' ,", forumInfo.Allowrss);
                forums.AppendFormat("[Allowhtml]='{0}' ,", forumInfo.Allowhtml);
                forums.AppendFormat("[Allowbbcode]='{0}' ,", forumInfo.Allowbbcode);
                forums.AppendFormat("[Allowimgcode]='{0}' ,", forumInfo.Allowimgcode);
                forums.AppendFormat("[Allowblog]='{0}' ,", forumInfo.Allowblog);
                forums.AppendFormat("[istrade]='{0}' ,", forumInfo.Istrade);
                forums.AppendFormat("[allowpostspecial]='{0}' ,", forumInfo.Allowpostspecial);
                forums.AppendFormat("[allowspecialonly]='{0}' ,", forumInfo.Allowspecialonly);
                forums.AppendFormat("[Alloweditrules]='{0}' ,", forumInfo.Alloweditrules);
                forums.AppendFormat("[allowthumbnail]='{0}' ,", forumInfo.Allowthumbnail);
                forums.AppendFormat("[Recyclebin]='{0}' ,", forumInfo.Recyclebin);
                forums.AppendFormat("[Modnewposts]='{0}' ,", forumInfo.Modnewposts);
                forums.AppendFormat("[Modnewtopics]='{0}' ,", forumInfo.Modnewtopics);
                forums.AppendFormat("[Jammer]='{0}' ,", forumInfo.Jammer);
                forums.AppendFormat("[Disablewatermark]='{0}' ,", forumInfo.Disablewatermark);
                forums.AppendFormat("[Inheritedmod]='{0}' ,", forumInfo.Inheritedmod);
                forums.AppendFormat("[allowtag]='{0}' ,", forumInfo.Allowtag);
            }
            if (forums.ToString().EndsWith(","))
                forums.Remove(forums.Length - 1, 1);

            forums.AppendFormat("WHERE [fid] IN ({0})", fidList);

            forumfields.AppendFormat("UPDATE [{0}forumfields] SET ", BaseConfigs.GetTablePrefix);
            if (bsp.SetPassWord)
                forumfields.AppendFormat("[password]='{0}' ,", forumInfo.Password);

            if (bsp.SetAttachExtensions)
                forumfields.AppendFormat("[attachextensions]='{0}' ,", forumInfo.Attachextensions);

            if (bsp.SetPostCredits)
                forumfields.AppendFormat("[postcredits]='{0}' ,", forumInfo.Postcredits);

            if (bsp.SetReplyCredits)
                forumfields.AppendFormat("[replycredits]='{0}' ,", forumInfo.Replycredits);

            if (bsp.SetViewperm)
                forumfields.AppendFormat("[Viewperm]='{0}' ,", forumInfo.Viewperm);

            if (bsp.SetPostperm)
                forumfields.AppendFormat("[Postperm]='{0}' ,", forumInfo.Postperm);

            if (bsp.SetReplyperm)
                forumfields.AppendFormat("[Replyperm]='{0}' ,", forumInfo.Replyperm);

            if (bsp.SetGetattachperm)
                forumfields.AppendFormat("[Getattachperm]='{0}' ,", forumInfo.Getattachperm);

            if (bsp.SetPostattachperm)
                forumfields.AppendFormat("[Postattachperm]='{0}' ,", forumInfo.Postattachperm);

            if (forumfields.ToString().EndsWith(","))
                forumfields.Remove(forumfields.Length - 1, 1);

            forumfields.AppendFormat("WHERE [fid] IN ({0})", fidList);

            SqlConnection conn = new SqlConnection(DbHelper.ConnectionString);
            conn.Open();
            using (SqlTransaction trans = conn.BeginTransaction())
            {
                try
                {
                    if (forums.ToString().IndexOf("SET WHERE") < 0)
                        DbHelper.ExecuteNonQuery(trans, CommandType.Text, forums.ToString());

                    if (forumfields.ToString().IndexOf("SET WHERE") < 0)
                        DbHelper.ExecuteNonQuery(trans, CommandType.Text, forumfields.ToString());

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    return false;
                }
            }
            return true;
        }

        public IDataReader GetTopForumFids(int lastFid, int statCount)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@lastfid", (DbType)NpgsqlDbType.Integer, 4, lastFid),
                                        DbHelper.MakeInParam("@statcount", (DbType)NpgsqlDbType.Integer, 4, statCount),
                                    };
            return DbHelper.ExecuteReader(CommandType.Text, string.Format("SELECT   [fid] FROM [dnt_forums] WHERE [fid] > @lastfid   limit " + statCount.ToString(), BaseConfigs.GetTablePrefix), parms);
        }

        public DataTable GetOnlineList()
        {
            string commandText = string.Format("SELECT [groupid],(SELECT  [grouptitle]  FROM [{0}usergroups] WHERE [{0}usergroups].[groupid]=[{0}onlinelist].[groupid]) AS GroupName ,[displayorder],[title],[img] FROM [{0}onlinelist] ORDER BY [groupid] ASC  LIMIT 1",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public int UpdateOnlineList(int groupId, int displayOrder, string img, string title)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@groupid", (DbType)NpgsqlDbType.Integer, 4, groupId),
                                        DbHelper.MakeInParam("@displayorder", (DbType)NpgsqlDbType.Integer, 4, displayOrder),
                                        DbHelper.MakeInParam("@img", (DbType)NpgsqlDbType.Varchar, 50, img),
                                        DbHelper.MakeInParam("@title", (DbType)NpgsqlDbType.Varchar, 50, title)
                                    };
            string commandText = string.Format("UPDATE [{0}onlinelist] SET [displayorder]=@displayorder, [title]=@title, [img]=@img  WHERE [groupid]=@groupid",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }



        public void UpdateBadWords(string find, string replacement)
        {
            string commandText = string.Format("UPDATE [{0}words] set [replacement]='{1}' WHERE [find] ='{2}'",
                                                BaseConfigs.GetTablePrefix,
                                                replacement,
                                                find);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public int UpdateWord(int id, string find, string replacement)
        {
            DbParameter[] parms = {
                    DbHelper.MakeInParam("@id", (DbType)NpgsqlDbType.Integer, 4, id),
					DbHelper.MakeInParam("@find", (DbType)NpgsqlDbType.Varchar, 255, find),
					DbHelper.MakeInParam("@replacement", (DbType)NpgsqlDbType.Varchar, 255, replacement)
				};
            string commandText = string.Format("UPDATE [{0}words] SET [find]=@find, [replacement]=@replacement WHERE [id]=@id",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public int DeleteWords(string idList)
        {
            string commandText = string.Format("DELETE FROM [{0}words]  WHERE [ID] IN ({1})", BaseConfigs.GetTablePrefix, idList);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public int AddWord(string userName, string find, string replacement)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@username", (DbType)NpgsqlDbType.Varchar, 20, userName),
                                        DbHelper.MakeInParam("@find", (DbType)NpgsqlDbType.Varchar, 255, find),
                                        DbHelper.MakeInParam("@replacement", (DbType)NpgsqlDbType.Varchar, 255, replacement)
                                    };
            string commandText = string.Format("INSERT INTO [{0}words] ([admin], [find], [replacement]) VALUES (@username,@find,@replacement)",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public DataTable GetTopicTypes(string searthKeyWord)
        {
            string commandText = string.Format("SELECT [typeid] AS id,[name],[displayorder],[description] FROM [{0}topictypes] {1} {2}",
                                        BaseConfigs.GetTablePrefix,
                                        !Utils.StrIsNullOrEmpty(searthKeyWord) ? " WHERE [name] LIKE '%" + searthKeyWord + "%' " : "",
                                        "ORDER BY [displayorder] ASC");
            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }

        public DataTable GetExistTopicTypeOfForum()
        {
            string commandText = string.Format("SELECT [fid],[topictypes] FROM [{0}forumfields] WHERE [topictypes] NOT LIKE ''",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public void UpdateTopicTypeForForum(string topicTypes, int fid)
        {
            DbParameter[] parms = { 
                                        DbHelper.MakeInParam("@topictypes", (DbType)NpgsqlDbType.Text, 0, topicTypes),
                                        DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, fid)
                                    };
            string commandText = string.Format("UPDATE [{0}forumfields] SET [topictypes]=@topictypes WHERE [fid]=@fid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void UpdateTopicTypes(string name, int displayOrder, string description, int typeId)
        {
            DbParameter[] parms = {
				DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Varchar,100, name),
				DbHelper.MakeInParam("@displayorder", (DbType)NpgsqlDbType.Integer,4,displayOrder),
				DbHelper.MakeInParam("@description", (DbType)NpgsqlDbType.Varchar,500,description),
				DbHelper.MakeInParam("@typeid", (DbType)NpgsqlDbType.Integer,4,typeId)
								   };
            string commandText = string.Format("UPDATE [{0}topictypes] SET [name]=@name ,[displayorder]=@displayorder, [description]=@description WHERE [typeid]=@typeid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void AddTopicTypes(string typeName, int displayOrder, string description)
        {
            DbParameter[] parms = {
				DbHelper.MakeInParam("@name",(DbType)NpgsqlDbType.Varchar,100, typeName),
				DbHelper.MakeInParam("@displayorder",(DbType)NpgsqlDbType.Integer,4,displayOrder),
				DbHelper.MakeInParam("@description",(DbType)NpgsqlDbType.Varchar,500,description)
								  };
            string commandText = string.Format("INSERT INTO [{0}topictypes] ([name],[displayorder],[description]) VALUES(@name,@displayorder,@description)",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        public void DeleteTopicTypesByTypeidlist(string typeIdList)
        {
            string commandText = string.Format("DELETE FROM [{0}topictypes]  WHERE [typeid] IN ({1})",
                                                BaseConfigs.GetTablePrefix,
                                                typeIdList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public DataTable GetForumNameIncludeTopicType()
        {
            string commandText = string.Format("SELECT f1.[fid],[name],[topictypes] FROM [{0}forums] AS f1 LEFT JOIN [{0}forumfields] AS f2 ON f1.fid=f2.fid",
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        public void ClearTopicTopicType(int typeId)
        {
            string commandText = string.Format("UPDATE [{0}topics] SET [typeid]=0 Where [typeid]={1}",
                                                BaseConfigs.GetTablePrefix, typeId);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public IDataReader GetTopicsIdentifyItem()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}topicidentify]", DbFields.TOPIC_IDENTIFY, BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteReaderInMasterDB(CommandType.Text, commandText);
        }

        public string ResetTopTopicListSql(int layer, string fid, string parenTidList)
        {

            string filterexpress = "";

            switch (layer)
            {
                case 0:
                    filterexpress = string.Format("[fid]<>{0} AND (',' + TRIM([parentidlist]) + ',' LIKE '%,{1},%')", fid.ToString(), RegEsc(fid.ToString()));
                    break;
                case 1:
                    filterexpress = parenTidList.ToString().Trim();
                    if (filterexpress != string.Empty)
                    {
                        filterexpress = string.Format("[fid]<>{0} AND ([fid]={1} OR (',' + TRIM([parentidlist]) + ',' LIKE '%,{2},%'))",
                                fid.ToString().Trim(), filterexpress, RegEsc(filterexpress));
                    }
                    else
                    {
                        filterexpress = string.Format("[fid]<>{0} AND (',' + TRIM([parentidlist]) + ',' LIKE '%,{1},%')",
                                fid.ToString().Trim(), RegEsc(filterexpress));
                    }
                    break;
                default:
                    filterexpress = parenTidList.ToString().Trim();
                    if (filterexpress != string.Empty)
                    {
                        filterexpress = Utils.CutString(filterexpress, 0, filterexpress.IndexOf(","));
                        filterexpress = string.Format("[fid]<>{0} AND ([fid]={1} OR (',' + TRIM([parentidlist]) + ',' LIKE '%,{2},%'))",
                                fid.ToString().Trim(), filterexpress, RegEsc(filterexpress));
                    }
                    else
                    {
                        filterexpress = string.Format("[fid]<>{0} AND (',' + TRIM([parentidlist]) + ',' LIKE '%,{1},%')",
                                fid.ToString().Trim(), RegEsc(filterexpress));
                    }
                    break;
            }

            return filterexpress;
        }

        public string ShowForumCondition(int sqlId, int cond)
        {
            string sql = null;
            switch (sqlId)
            {
                case 1:
                    sql = " AND [typeid]=";
                    break;
                case 2:
                    sql = " AND [postdatetime]>='" + DateTime.Now.AddDays(-1 * cond).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    break;
                //case 2:
                //    sql = DateTime.Now.AddDays(-1 * cond).ToString("yyyy-MM-dd HH:mm:ss");
                //    break;
                case 3:
                    sql = "tid";
                    break;

                case 4:
                    sql = "views";
                    break;
                case 5:
                    sql = "replies";
                    break;
            }
            return sql;
        }

        public string DelVisitLogCondition(string deleteMod, string visitId, string deleteNum, string deleteFrom)
        {
            string condition = null;
            switch (deleteMod)
            {
                case "chkall":
                    if (visitId != "")
                        condition = string.Format(" [visitid] IN ({0})", visitId);
                    break;
                case "deleteNum":
                    if (deleteNum != "" && Utils.IsNumeric(deleteNum))
                        condition = string.Format(" [visitid] NOT IN (SELECT   [visitid] FROM [{1}adminvisitlog] ORDER BY [visitid] DESC) LIMIT  {0}",
                                                   deleteNum,
                                                   BaseConfigs.GetTablePrefix);
                    break;
                case "deleteFrom":
                    if (deleteFrom != "")
                        condition = " [postdatetime]<'" + deleteFrom + "'";
                    break;
            }
            return condition;
        }



        public DataTable GetAttachDataTable(string condition, string postName)
        {
            string commandText = string.Format("SELECT [aid], [attachment], [filename], (SELECT   [poster] FROM [{0}] WHERE [{0}].[pid]=[{1}attachments].[pid]) AS [poster],(Select TOP 1 [title] FROM [{1}topics] WHERE [{1}topics].[tid]=[{1}attachments].[tid]) AS [topictitle], [filesize],[downloads]  FROM [{1}attachments] {2}  LIMIT 1",
                                                postName,
                                                BaseConfigs.GetTablePrefix,
                                                condition);
            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }


        public bool AuditTopicCount(string condition)
        {
            string commandText = string.Format("SELECT COUNT(tid) FROM [{0}topics] WHERE {1}", BaseConfigs.GetTablePrefix, condition);
            return DbHelper.ExecuteDataset(commandText).Tables[0].Rows[0][0].ToString() == "0";
        }

        public string AuditTopicBindStr(string condition)
        {
            return string.Format("SELECT {0} FROM [{1}topics] WHERE {2}", DbFields.TOPICS, BaseConfigs.GetTablePrefix, condition);
        }

        public DataTable AuditTopicBind(string condition)
        {
            return DbHelper.ExecuteDataset(AuditTopicBindStr(condition)).Tables[0];
        }

        public DataTable AuditNewUserClear(string searchUser, string regBefore, string regIp)
        {
            string commandText = string.Format("SELECT {0} FROM [{1}users] WHERE [groupid]=8 {2} {3} {4}",
                                                 DbFields.USERS,
                                                 BaseConfigs.GetTablePrefix,
                                                 !Utils.StrIsNullOrEmpty(searchUser) ? " AND [username] LIKE '%" + searchUser + "%'" : "",
                                                 !Utils.StrIsNullOrEmpty(regBefore) ? " AND [joindate]<='" + DateTime.Now.AddDays(-Convert.ToDouble(regBefore)).ToString("yyyy-MM-dd HH:mm:ss") + "' " : "",
                                                 !Utils.StrIsNullOrEmpty(regIp) ? " AND [regip] LIKE '" + RegEsc(regIp) + "%'" : "");
            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }

        public string DelMedalLogCondition(string deleteMode, string id, string deleteNum, string deleteFrom)
        {
            string condition = "";
            switch (deleteMode)
            {
                case "chkall":
                    if (id != "")
                        condition = string.Format(" [id] IN ({0})", id);
                    break;
                case "deleteNum":
                    if (deleteNum != "" && Utils.IsNumeric(deleteNum))
                        condition = string.Format(" [id] NOT IN (SELECT TOP {0} [id] FROM [{1}medalslog] ORDER BY [id] DESC)",
                                                    deleteNum,
                                                    BaseConfigs.GetTablePrefix);
                    break;
                case "deleteFrom":
                    if (deleteFrom != "")
                        condition = string.Format(" [postdatetime]<'{0}'", DateTime.Parse(deleteFrom).ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
            }
            return condition;
        }


        public string DelModeratorManageCondition(string deleteMode, string id, string deleteNum, string deleteFrom)
        {
            string condition = "";
            switch (deleteMode)
            {
                case "chkall":
                    if (id != "")
                        condition = string.Format(" [id] IN ({0})", id);
                    break;
                case "deleteNum":
                    if (deleteNum != "" && Utils.IsNumeric(deleteNum))
                        condition = string.Format(" [id] NOT IN (SELECT TOP {0} [id] FROM [{1}moderatormanagelog] ORDER BY [id] DESC)",
                                                    deleteNum,
                                                    BaseConfigs.GetTablePrefix);
                    break;
                case "deleteFrom":
                    if (deleteFrom != "")
                        condition = string.Format(" [postdatetime]<'{0}'", DateTime.Parse(deleteFrom).ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
            }
            return condition;
        }

        /// <summary>
        /// ����ָ��������ȡ������Ϣ�б�
        /// </summary>
        /// <param name="postTableName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public DataTable PostGridBind(string postTableName, string condition)
        {
            string commandText = string.Format("SELECT {0} FROM [{1}] WHERE {2}", DbFields.POSTS, postTableName, condition);
            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }


        public void UpdatePostSP()
        {
            #region ���·ֱ�Ĵ洢����
            foreach (DataRow dr in DatabaseProvider.GetInstance().GetDatechTableIds())
            {
                CreateStoreProc(TypeConverter.ObjectToInt(dr["id"]));
            }
            #endregion
        }

        public void CreateStoreProc(int tableListMaxId)
        {
            #region �����ֱ�洢����
            StringBuilder sb = new StringBuilder();

            string detachTablePath = "";
            if (DbHelper.ExecuteScalar(CommandType.Text, "SELECT @@VERSION").ToString().Substring(20, 24).Trim().IndexOf("2000") >= 0)
                detachTablePath = Utils.GetMapPath(BaseConfigs.GetForumPath.ToLower() + "config/detachtable_2000.config");
            else
                detachTablePath = Utils.GetMapPath(BaseConfigs.GetForumPath.ToLower() + "config/detachtable_2005.config");

            if (!File.Exists(detachTablePath))
            {
                throw new FileNotFoundException("�ֱ�洢�����ļ�������!");
            }
            using (StreamReader objReader = new StreamReader(detachTablePath, Encoding.UTF8))
            {
                sb.Append(objReader.ReadToEnd());
                objReader.Close();
            }
            sb.Replace("\"", "'").Replace("dnt_posts1", BaseConfigs.GetTablePrefix + "posts" + tableListMaxId);
            sb.Replace("maxtablelistid", tableListMaxId.ToString());
            sb.Replace("dnt_createpost", BaseConfigs.GetTablePrefix + "createpost" + tableListMaxId);
            sb.Replace("dnt_getfirstpostid", BaseConfigs.GetTablePrefix + "getfirstpost" + tableListMaxId + "id");
            //sb.Replace("dnt_getpostcount", BaseConfigs.GetTablePrefix + "getpost" + tableListMaxId + "count");
            sb.Replace("dnt_deletepostbypid", BaseConfigs.GetTablePrefix + "deletepost" + tableListMaxId + "bypid");
            sb.Replace("dnt_getposttree", BaseConfigs.GetTablePrefix + "getpost" + tableListMaxId + "tree");
            sb.Replace("dnt_getsinglepost", BaseConfigs.GetTablePrefix + "getsinglepost" + tableListMaxId);
            sb.Replace("dnt_updatepost", BaseConfigs.GetTablePrefix + "updatepost" + tableListMaxId);
            sb.Replace("dnt_getnewtopics", BaseConfigs.GetTablePrefix + "getnewtopics");
            sb.Replace("dnt_getpostlist1", BaseConfigs.GetTablePrefix + "getpostlist" + tableListMaxId);
            sb.Replace("dnt_deletetopicbytidlist1", BaseConfigs.GetTablePrefix + "deletetopicbytidlist" + tableListMaxId);
            sb.Replace("dnt_getreplypid1", BaseConfigs.GetTablePrefix + "getreplypid" + tableListMaxId);
            sb.Replace("dnt_getnewtopics1", BaseConfigs.GetTablePrefix + "getnewtopics" + tableListMaxId);
            sb.Replace("dnt_getlastpostlist1", BaseConfigs.GetTablePrefix + "getlastpostlist" + tableListMaxId);
            sb.Replace("dnt_getdebatepostlist1", BaseConfigs.GetTablePrefix + "getdebatepostlist" + tableListMaxId);
            sb.Replace("dnt_getpostcountbycondition1", BaseConfigs.GetTablePrefix + "getpostcountbycondition" + tableListMaxId);
            sb.Replace("dnt_getpostlistbycondition1", BaseConfigs.GetTablePrefix + "getpostlistbycondition" + tableListMaxId);
            sb.Replace("dnt_", BaseConfigs.GetTablePrefix);
            DatabaseProvider.GetInstance().CreatePostProcedure(sb.ToString());

            #endregion
        }

        public void UpdateMyTopic()
        {
            //�ؽ��ҵ������
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updatemytopic", BaseConfigs.GetTablePrefix));
        }

        public void UpdateMyPost()
        {
            //�ؽ��ҵ����ӱ�
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}updatemypost", BaseConfigs.GetTablePrefix));
        }


        public DataTable GetAllIdentify()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}topicidentify]", DbFields.TOPIC_IDENTIFY, BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public bool UpdateIdentifyById(int id, string name)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@identifyid", (DbType)NpgsqlDbType.Integer,4,id),
				DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Varchar,50, name)
			};
            string commandText = string.Format("SELECT COUNT(1) FROM [{0}topicidentify] WHERE [name]=@name AND [identifyid]<>@identifyid",
                                                BaseConfigs.GetTablePrefix);
            if (TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText, parms)) != 0)  //����ͬ�����ƴ��ڣ�����ʧ��
                return false;

            DbHelper.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE [{0}topicidentify] SET [name]=@name WHERE [identifyid]=@identifyid", BaseConfigs.GetTablePrefix), parms);
            return true;
        }

        public bool AddIdentify(string name, string fileName)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@name", (DbType)NpgsqlDbType.Varchar,50, name),
				DbHelper.MakeInParam("@filename", (DbType)NpgsqlDbType.Varchar,50,fileName),
			};
            string commandText = string.Format("SELECT COUNT(1) FROM [{0}topicidentify] WHERE [name]=@name", BaseConfigs.GetTablePrefix);
            if (TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText, parms)) != 0)  //����ͬ�����ƴ��ڣ�����ʧ��
                return false;

            commandText = string.Format("INSERT INTO [{0}topicidentify] ([name],[filename]) VALUES (@name,@filename)", BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
            return true;
        }

        public void DeleteIdentify(string idList)
        {
            string commandText = string.Format("DELETE [{0}topicidentify] WHERE [identifyid] IN ({1})", BaseConfigs.GetTablePrefix, idList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }


        public IDataReader GetAttachmentByUid(int uid, string extList, int pageIndex, int pageSize)
        {
            DbParameter[] parms = 
			{
                DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer,4,uid),
                DbHelper.MakeInParam("@extlist ", (DbType)NpgsqlDbType.Varchar,100,extList),
                DbHelper.MakeInParam("@pageindex", (DbType)NpgsqlDbType.Integer,4,pageIndex),
                DbHelper.MakeInParam("@pagesize", (DbType)NpgsqlDbType.Integer,4,pageSize)
			};
            return DbHelper.ExecuteReader(CommandType.StoredProcedure,
                                          string.Format("{0}getmyattachmentsbytype", BaseConfigs.GetTablePrefix),
                                          parms);
        }

        public int GetUserAttachmentCount(int uid)
        {
            string commandText = string.Format("SELECT COUNT(1) FROM [{0}myattachments] WHERE [UID]={1}", BaseConfigs.GetTablePrefix, uid);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText));
        }

        public int GetUserAttachmentCount(int uid, string extList)
        {
            string commandText = string.Format("select count(1) from [{0}myattachments] where [extname] IN ({1}) and [UID]={2}",
                                                BaseConfigs.GetTablePrefix,
                                                extList,
                                                uid);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText));
        }


        public IDataReader GetAttachmentByUid(int uid, int pageIndex, int pageSize)
        {
            DbParameter[] parms = 
			{
                DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer,4,uid),
                DbHelper.MakeInParam("@pageindex", (DbType)NpgsqlDbType.Integer,4,pageIndex),
                DbHelper.MakeInParam("@pagesize", (DbType)NpgsqlDbType.Integer,4,pageSize)
			};
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("{0}getmyattachments", BaseConfigs.GetTablePrefix), parms);
        }


        public void DelMyAttachmentByTid(string tidList)
        {
            string commandText = string.Format("DELETE FROM [{0}myattachments] WHERE [tid] IN ({1})", BaseConfigs.GetTablePrefix, tidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void DelMyAttachmentByPid(string pidList)
        {
            string commandText = string.Format("DELETE FROM [{0}myattachments] WHERE [pid] IN ({1})", BaseConfigs.GetTablePrefix, pidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public void DelMyAttachmentByAid(string aidList)
        {
            string commandText = string.Format("DELETE FROM [{0}myattachments] WHERE [aid] IN ({1})", BaseConfigs.GetTablePrefix, aidList);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public IDataReader GetHotTagsListForForum(int count)
        {
            string commandText = string.Format("SELECT {1} FROM [{2}tags] WHERE [fcount] > 0 AND [orderid] > -1 ORDER BY [orderid], [fcount] DESC  LIMIT {0}",
                                                count,
                                                DbFields.TAGS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteReaderInMasterDB(CommandType.Text, commandText);
        }

        /// <summary>
        /// ������̳Tag�б�
        /// </summary>
        /// <param name="tagname">��ѯ�ؼ���</param>
        /// <param name="type">ȫ��0 ����1 ����2</param>
        /// <returns></returns>
        public DataTable GetForumTags(string tagName, int type)
        {
            //type ȫ��0 ����1 ����2
            string commandText = string.Format("SELECT {0} FROM [{1}tags]  {2} ",
                                        DbFields.TAGS,
                                        BaseConfigs.GetTablePrefix,
                                        !Utils.StrIsNullOrEmpty(tagName) ? " WHERE [tagname] LIKE '%" + RegEsc(tagName) + "%'" : "");

            if (type == 1)
                commandText += !Utils.StrIsNullOrEmpty(tagName) ? " AND [orderid] < 0 " : " WHERE [orderid] < 0 ";
            else if (type == 2)
                commandText += !Utils.StrIsNullOrEmpty(tagName) ? " AND [orderid] >= 0" : " WHERE [orderid] >= 0 ";

            commandText += " ORDER BY [fcount] DESC";

            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public DataTable GetTopicNumber(string tagName, int from, int end, int type)
        {
            //type ȫ��0 ����1 ����
            string commandText = string.Format("SELECT {0} FROM [{1}tags] {2}",
                                                DbFields.TAGS,
                                                BaseConfigs.GetTablePrefix,
                                                !Utils.StrIsNullOrEmpty(tagName) ? " WHERE [tagname] LIKE '%" + RegEsc(tagName) + "%'" : "");

            if (type == 1)
            {
                if (tagName != "")
                    commandText += string.Format(" AND [orderid] < 0  AND [fcount] between {0} AND {1}", from, end);
                else
                    commandText += string.Format(" WHERE [orderid] < 0  AND [fcount] between {0} AND {1}", from, end);
            }
            else if (type == 2)
            {
                if (tagName != "")
                    commandText += string.Format(" AND [orderid] >= 0  AND [fcount]  between {0} AND {1}", from, end);
                else
                    commandText += string.Format(" WHERE [orderid] >= 0  AND [fcount]  between {0} AND {1}", from, end);
            }

            commandText += " ORDER BY [fcount] DESC";
            return DbHelper.ExecuteDataset(commandText).Tables[0];
        }


        public void UpdateForumTags(int tagId, int orderId, string color)
        {
            DbParameter[] parms = 
			{
				DbHelper.MakeInParam("@orderid", (DbType)NpgsqlDbType.Integer,4, orderId),
                DbHelper.MakeInParam("@color", (DbType)NpgsqlDbType.Char,6, color),
				DbHelper.MakeInParam("@tagid", (DbType)NpgsqlDbType.Integer,4,tagId)
			};
            string commandText = string.Format("UPDATE [{0}tags] SET [orderid]=@orderid,[color]=@color WHERE [tagid]=@tagid",
                                                BaseConfigs.GetTablePrefix);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        /// <summary>
        /// �������п��Ű���б�
        /// </summary>
        /// <returns></returns>
        public DataTable GetOpenForumList()
        {
            string commandText = string.Format("SELECT [parentid],[fid],[name] FROM (SELECT {0} FROM [{1}forums] f  LEFT JOIN [{1}forumfields] ff ON f.[fid]=ff.[fid]) f  WHERE [status]=1 AND ([permuserlist] IS NULL OR [permuserlist] LIKE '') AND ([viewperm] IS NULL OR [viewperm] LIKE ''  OR CHARINDEX(',7,',','+CONVERT(VARCHAR(1000),[viewperm])+',')<>0) ORDER BY [displayorder] ASC",
                                                DbFields.FORUMS_JOIN_FIELDS,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }


        public IDataReader GetHotTopics(int count)
        {
            string commandText = string.Format("SELECT  [views], [tid], [title] FROM [{1}topics] WHERE [displayorder]>=0 ORDER BY [views] DESC  limit   {0}",
                                                count,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        public IDataReader GetHotReplyTopics(int count)
        {
            string commandText = string.Format("SELECT  [replies], [tid], [title] FROM [{1}topics] WHERE [displayorder]>=0 ORDER BY [replies] DESC  limit   {0}",
                                                count,
                                                BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        public IDataReader GetTopicBonusLogs(int tid, string postTableId)
        {
            //string commandText = string.Format("SELECT [tid],[authorid],[answerid],[answername],[extid],SUM(bonus) AS [bonus] FROM [{0}bonuslog] WHERE [tid]={1} GROUP BY [answerid],[authorid],[tid],[answername],[extid]",
            //                                    BaseConfigs.GetTablePrefix,
            //                                    tid);
            string commandText = string.Format("SELECT b.[tid],[authorid],[answerid],[answername],b.[pid],[extid],[bonus],[isbest],[message] FROM [{0}bonuslog] AS b LEFT JOIN [{0}posts{1}] AS p ON b.[pid]=p.[pid] WHERE b.[tid]={2} AND [bonus]<>0 ORDER BY [isbest] DESC",
                                                BaseConfigs.GetTablePrefix, postTableId, tid);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        public IDataReader GetTopicBonusLogsByPost(int tid)
        {
            string commandText = string.Format("SELECT [pid],[isbest],[bonus],[extid] FROM [{0}bonuslog] WHERE [tid]={1}",
                                                BaseConfigs.GetTablePrefix,
                                                tid);
            return DbHelper.ExecuteReader(CommandType.Text, commandText);
        }

        public DataTable GetAllOpenForum()
        {
            string commandText = string.Format("SELECT {0} FROM [{1}forums] f LEFT JOIN [{1}forumfields] ff ON f.[fid] = ff.[fid] WHERE f.[autoclose]=0 AND ff.[password]='' AND ff.[redirect]=''",
                                        DbFields.FORUMS_JOIN_FIELDS,
                                        BaseConfigs.GetTablePrefix);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        public int GetForumsLastPostTid(string fidList)
        {
            string commandText = string.Format("SELECT [tid] FROM [{0}topics] WHERE [lastpostid] = (SELECT MAX([lastpostid]) FROM [{0}topics] WHERE [fid] IN({1}) AND [displayorder]>-1 AND [closed]=0)",
                                        BaseConfigs.GetTablePrefix,
                                        fidList);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText));
        }

        public void UpdateLastPost(int lastTid, string lastTitle, string lastPost, int lastPosterId, string lastPoster, int fid)
        {
            DbParameter[] parms ={
                                     DbHelper.MakeInParam("@lasttid", (DbType)NpgsqlDbType.Integer, 4, lastTid),
                                     DbHelper.MakeInParam("@lasttitle", (DbType)NpgsqlDbType.Char, 60, lastTitle),
                                     DbHelper.MakeInParam("@lastpost", (DbType)NpgsqlDbType.Timestamp, 8, lastPost),
                                     DbHelper.MakeInParam("@lastposterid", (DbType)NpgsqlDbType.Integer, 4, lastPosterId),
                                     DbHelper.MakeInParam("@lastposter", (DbType)NpgsqlDbType.Char, 20, lastPoster),
                                     DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, fid)
                                 };
            string commandText = string.Format("UPDATE [{0}forums] SET [lasttid] = @lasttid, [lasttitle] = @lasttitle, [lastpost] = @lastpost, [lastposterid] = @lastposterid, [lastposter] = @lastposter WHERE [fid] = @fid",
                                                BaseConfigs.GetTablePrefix,
                                                fid);
            DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        }

        //public void UpdateLastPost(int lastTid, string lastTitle, string lastPost, int lastPosterId, string lastPoster, int fid, string parenTidList)
        //{
        //    DbParameter[] parms ={
        //                             DbHelper.MakeInParam("@lasttid", (DbType)NpgsqlDbType.Integer, 4, lastTid),
        //                             DbHelper.MakeInParam("@lasttitle", (DbType)NpgsqlDbType.Char, 60, lastTitle),
        //                             DbHelper.MakeInParam("@lastpost", (DbType)NpgsqlDbType.Timestamp, 8, lastPost),
        //                             DbHelper.MakeInParam("@lastposterid", (DbType)NpgsqlDbType.Integer, 4, lastPosterId),
        //                             DbHelper.MakeInParam("@lastposter", (DbType)NpgsqlDbType.Char, 20, lastPoster),
        //                             DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, fid)
        //                         };
        //    string commandText = string.Format("UPDATE [{0}forums] SET [lasttid] = @lasttid, [lasttitle] = @lasttitle, [lastpost] = @lastpost, [lastposterid] = @lastposterid, [lastposter] = @lastposter WHERE [fid] = @fid OR [fid] IN ({1})",
        //                                        BaseConfigs.GetTablePrefix,
        //                                        parenTidList);
        //    DbHelper.ExecuteNonQuery(CommandType.Text, commandText, parms);
        //}

        //public void UpdateLastPost(ForumInfo forumInfo, PostInfo postInfo)
        //{
        //    UpdateLastPost(postInfo.Tid, postInfo.Topictitle, postInfo.Postdatetime.ToString(), postInfo.Posterid, postInfo.Poster, forumInfo.Fid, forumInfo.Parentidlist);
        //}

        public void UpdateLastPost(ForumInfo forumInfo, PostInfo postInfo)
        {
            UpdateLastPost(postInfo.Tid, postInfo.Topictitle, postInfo.Postdatetime.ToString(), postInfo.Posterid, postInfo.Poster, forumInfo.Fid);
        }

        /// <summary>
        /// �������а���������˵���Ϣ(��̨���°�������)
        /// </summary>
        public void ResetLastPostInfo()
        {
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}resetlastpostinfo", BaseConfigs.GetTablePrefix));
        }

        /// <summary>
        /// �ж�ָ���û����Ƿ��Ѵ���
        /// </summary>
        /// <param name="uid">�û�id</param>
        /// <returns>����Ѵ��ڸ�rewritename�򷵻�true, ���򷵻�false</returns>
        public bool CheckForumRewriteNameExists(string rewriteName)
        {
            DbParameter[] parms = { 
                                       DbHelper.MakeInParam("@rewritename", (DbType)NpgsqlDbType.Varchar, 20, rewriteName) 
                                  };
            string commandText = string.Format("SELECT COUNT(1) FROM [{0}forumfields] WHERE [rewritename]=@rewritename",
                                                BaseConfigs.GetTablePrefix);
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.Text, commandText, parms)) >= 1;
        }

        /// <summary>
        /// ��������ȡ����
        /// </summary>
        /// <param name="condition">����</param>
        /// <returns></returns>
        public DataTable GetTopicsByCondition(string condition)
        {
            string commandText = string.Format("SELECT {0} FROM [{1}topics] WHERE {2}",
                                                DbFields.TOPICS,
                                                BaseConfigs.GetTablePrefix,
                                                condition);
            return DbHelper.ExecuteDataset(CommandType.Text, commandText).Tables[0];
        }

        /// <summary>
        /// �����ݿ��в����µ�������
        /// </summary>
        /// <param name="code"></param>
        /// <param name="userid"></param>
        /// <param name="createtime"></param>
        /// <param name="invalidtime"></param>
        /// <param name="maxusecount"></param>
        public int CreateInviteCode(InviteCodeInfo inviteCode)
        {
            DbParameter[] parms = { 
                                       DbHelper.MakeInParam("@code",(DbType)NpgsqlDbType.Char, 7, inviteCode.Code),
                                       DbHelper.MakeInParam("@creatorid",(DbType)NpgsqlDbType.Integer, 4, inviteCode.CreatorId),
                                       DbHelper.MakeInParam("@creator",(DbType)NpgsqlDbType.Char, 20, inviteCode.Creator),
                                       DbHelper.MakeInParam("@createtime",(DbType)NpgsqlDbType.Timestamp, 4, inviteCode.CreateTime),
                                       DbHelper.MakeInParam("@expiretime",(DbType)NpgsqlDbType.Timestamp, 4, inviteCode.ExpireTime),
                                       DbHelper.MakeInParam("@maxcount",(DbType)NpgsqlDbType.Integer, 4,inviteCode.MaxCount ),
                                       DbHelper.MakeInParam("@invitetype",(DbType)NpgsqlDbType.Integer, 4, inviteCode.InviteType)
                                  };
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.StoredProcedure, string.Format("[{0}createinvitecode]", BaseConfigs.GetTablePrefix), parms));
        }

        /// <summary>
        /// ����������Ƿ���������ݿ���
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsInviteCodeExist(string code)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@code",(DbType)NpgsqlDbType.Char, 7, code)
                                  };
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.StoredProcedure, string.Format("[{0}isinvitecodeexist]", BaseConfigs.GetTablePrefix), parms)) >= 1;
        }

        /// <summary>
        /// ͨ��ID��ȡ������
        /// </summary>
        /// <param name="inviteid"></param>
        /// <returns></returns>
        public IDataReader GetInviteCodeByUid(int userId)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@searchtype",(DbType)NpgsqlDbType.Char, 10, "uid"),
                                      DbHelper.MakeInParam("@searchkey",(DbType)NpgsqlDbType.Char, 20, userId)
                                  };
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("[{0}getinvitecode]", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ͨ��������ID��ȡ������
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public IDataReader GetInviteCodeById(int inviteId)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@searchtype",(DbType)NpgsqlDbType.Char, 10, "id"),
                                      DbHelper.MakeInParam("@searchkey",(DbType)NpgsqlDbType.Char, 20, inviteId)
                                  };
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("[{0}getinvitecode]", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ͨ���ַ����ȡ������
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public IDataReader GetInviteCodeByCode(string code)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@searchtype",(DbType)NpgsqlDbType.Char, 10, "code"),
                                      DbHelper.MakeInParam("@searchkey",(DbType)NpgsqlDbType.Char, 20, code)
                                  };
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("[{0}getinvitecode]", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ɾ��������
        /// </summary>
        /// <param name="inviteid"></param>
        public void DeleteInviteCode(int inviteId)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@id",(DbType)NpgsqlDbType.Integer, 4, inviteId),
                                  };
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("[{0}deleteinvitecode]", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ����������ĳɹ�ע�����
        /// </summary>
        /// <param name="inviteid"></param>
        public void UpdateInviteCodeSuccessCount(int inviteId)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@id",(DbType)NpgsqlDbType.Integer, 4, inviteId),
                                  };
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("[{0}updateinvitecodesuccesscount]", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ��ȡ�û��������б�(���ʽ)
        /// </summary>
        /// <param name="creatorid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public IDataReader GetUserInviteCodeList(int creatorId, int pageIndex)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@creatorid",(DbType)NpgsqlDbType.Integer,4,creatorId),
                                      DbHelper.MakeInParam("@pageindex",(DbType)NpgsqlDbType.Integer,4,pageIndex)
                                  };
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("{0}getinvitecodelistbyuid", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ��ȡ�û�ӵ�е����������(���ʽ)
        /// </summary>
        /// <param name="creatorid"></param>
        /// <returns></returns>
        public int GetUserInviteCodeCount(int creatorId)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@creatorid",(DbType)NpgsqlDbType.Integer, 4, creatorId),
                                  };
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.StoredProcedure, string.Format("[{0}getuserinvitecodecount]", BaseConfigs.GetTablePrefix), parms));
        }

        /// <summary>
        /// ��ȡ�û��������������������
        /// </summary>
        /// <param name="creatorId"></param>
        /// <returns></returns>
        public int GetTodayUserCreatedInviteCode(int creatorId)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@creatorid",(DbType)NpgsqlDbType.Integer, 4, creatorId),
                                  };
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.StoredProcedure, string.Format("[{0}gettodayusercreatedinvitecode]", BaseConfigs.GetTablePrefix), parms));
        }

        /// <summary>
        /// ����û��ѹ��ڵ�������(���ʽ)
        /// </summary>
        /// <param name="creatorId"></param>
        /// <returns></returns>
        public int ClearExpireInviteCode()
        {
            return TypeConverter.ObjectToInt(DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("[{0}clearexpireinvitecode]", BaseConfigs.GetTablePrefix)));
        }

        /// <summary>
        /// �������а���������
        /// </summary>
        public void ResetForumsTopics()
        {
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}resetforumstopics", BaseConfigs.GetTablePrefix));
        }

        /// <summary>
        /// �������а��Ľ��շ�����
        /// </summary>
        public void ResetTodayPosts()
        {
            DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("{0}resettodayposts", BaseConfigs.GetTablePrefix));
        }

        /// <summary>
        /// �������ֶ�����Ϣ
        /// </summary>
        /// <param name="creditOrderInfo"></param>
        /// <returns></returns>
        public int CreateCreditOrder(CreditOrderInfo creditOrderInfo)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@ordercode",(DbType)NpgsqlDbType.Char, 32, creditOrderInfo.OrderCode),
                                      DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,creditOrderInfo.Uid),
                                      DbHelper.MakeInParam("@buyer",(DbType)NpgsqlDbType.Char,20,creditOrderInfo.Buyer),
                                      DbHelper.MakeInParam("@paytype",(DbType)NpgsqlDbType.Smallint,2,creditOrderInfo.PayType),
                                      DbHelper.MakeInParam("@price",(DbType)NpgsqlDbType.Real,8,creditOrderInfo.Price),
                                      DbHelper.MakeInParam("@orderstatus",(DbType)NpgsqlDbType.Smallint,2,creditOrderInfo.OrderStatus),
                                      DbHelper.MakeInParam("@credit",(DbType)NpgsqlDbType.Smallint,2,creditOrderInfo.Credit),
                                      DbHelper.MakeInParam("@amount",(DbType)NpgsqlDbType.Integer,4,creditOrderInfo.Amount)
                                  };
            return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("[{0}createorder]", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ��ȡ���ϲ�ѯ�����Ķ�������
        /// </summary>
        /// <param name="status"></param>
        /// <param name="orderId"></param>
        /// <param name="tradeNo"></param>
        /// <param name="buyer"></param>
        /// <param name="submitStartTime"></param>
        /// <param name="submitLastTime"></param>
        /// <param name="confirmStartTime"></param>
        /// <param name="confirmLastTime"></param>
        /// <returns></returns>
        public int GetCreditOrderCount(int status, int orderId, string tradeNo, string buyer, string submitStartTime, string submitLastTime, string confirmStartTime, string confirmLastTime)
        {
            string condition = "";
            if (status >= 0 || orderId > 0 || !string.IsNullOrEmpty(tradeNo) || !string.IsNullOrEmpty(buyer) ||
                !string.IsNullOrEmpty(submitStartTime) || !string.IsNullOrEmpty(submitLastTime) || !string.IsNullOrEmpty(confirmStartTime) || !string.IsNullOrEmpty(confirmLastTime))
            {
                condition += " WHERE 1 = 1 ";
                if (status >= 0)
                    condition += "AND [orderstatus] =" + status + " ";
                if (orderId > 0)
                    condition += "AND [orderid] ='" + orderId + "' ";
                if (tradeNo != "")
                    condition += "AND [tradeno] ='" + tradeNo + "' ";
                if (buyer != "")
                {
                    string buyerstr = "";
                    foreach (string str in Utils.SplitString(buyer, ","))
                    {
                        buyerstr += "'" + str + "',";
                    }
                    condition += "AND [buyer] IN(" + buyerstr.TrimEnd(',') + ") ";
                }
                if (submitStartTime != "")
                    condition += "AND [createdtime] > '" + submitStartTime + "' ";
                if (submitLastTime != "")
                {
                    DateTime time = DateTime.Parse(submitLastTime);
                    condition += "AND [createdtime] < '" + time.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                if (confirmStartTime != "")
                    condition += "AND [confirmedtime] > '" + confirmStartTime + "' ";
                if (confirmLastTime != "")
                {
                    DateTime time = DateTime.Parse(confirmLastTime);
                    condition += "AND [confirmedtime] < '" + time.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
            }
            DbParameter[] parms = {
                                     DbHelper.MakeInParam("@searchcondition",(DbType)NpgsqlDbType.Varchar, 1000,condition)
                                  };
            return TypeConverter.ObjectToInt(DbHelper.ExecuteScalar(CommandType.StoredProcedure, string.Format("[{0}getorderscount]", BaseConfigs.GetTablePrefix), parms));
        }

        /// <summary>
        /// ��ȡ���ϲ�ѯ�����Ķ���
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="status"></param>
        /// <param name="orderId"></param>
        /// <param name="tradeNo"></param>
        /// <param name="buyer"></param>
        /// <param name="submitStartTime"></param>
        /// <param name="submitLastTime"></param>
        /// <param name="confirmStartTime"></param>
        /// <param name="confirmLastTime"></param>
        /// <returns></returns>
        public IDataReader GetCreditOrderList(int pageIndex, int status, int orderId, string tradeNo, string buyer, string submitStartTime, string submitLastTime, string confirmStartTime, string confirmLastTime)
        {
            string condition = " WHERE 1 = 1 ";
            if (status >= 0 || orderId > 0 || !string.IsNullOrEmpty(tradeNo) || !string.IsNullOrEmpty(buyer) || !string.IsNullOrEmpty(submitStartTime)
                || !string.IsNullOrEmpty(submitLastTime) || !string.IsNullOrEmpty(confirmStartTime) || !string.IsNullOrEmpty(confirmLastTime))
            {
                if (orderId > 0)
                    condition += "AND [orderid] ='" + orderId + "' ";
                if (tradeNo != "")
                    condition += "AND [tradeno] ='" + tradeNo + "' ";
                if (status >= 0)
                    condition += "AND [orderstatus] =" + status + " ";
                if (buyer != "")
                {
                    string buyerstr = "";
                    foreach (string str in Utils.SplitString(buyer, ","))
                    {
                        buyerstr += "'" + str + "',";
                    }
                    condition += "AND [buyer] IN(" + buyerstr.TrimEnd(',') + ") ";
                }
                if (submitStartTime != "")
                    condition += "AND [createdtime] > '" + submitStartTime + "' ";
                if (submitLastTime != "")
                {
                    DateTime time = DateTime.Parse(submitLastTime);
                    condition += "AND [createdtime] < '" + time.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                if (confirmStartTime != "")
                    condition += "AND [confirmedtime] > '" + confirmStartTime + "' ";
                if (confirmLastTime != "")
                {
                    DateTime time = DateTime.Parse(confirmLastTime);
                    condition += "AND [confirmedtime] < '" + time.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
            }
            if (pageIndex > 1)
                condition += "AND [orderid] NOT IN(SELECT TOP " + (pageIndex - 1) * 20 + " [orderid] FROM " + string.Format("[{0}orders]", BaseConfigs.GetTablePrefix) + " " + condition + " ORDER BY [orderid] DESC)";
            DbParameter[] parms = {
                                     DbHelper.MakeInParam("@searchcondition",(DbType)NpgsqlDbType.Varchar, 1000,condition)
                                  };
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("[{0}getorderlist]", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ͨ��ordercode��ȡ���ֶ�����Ϣ
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public IDataReader GetCreditOrderByOrderCode(string orderCode)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@ordercode",(DbType)NpgsqlDbType.Char,32,orderCode)
                                  };
            return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("[{0}getorderbyordercode]", BaseConfigs.GetTablePrefix), parms);
        }

        /// <summary>
        /// ���»��ֶ�����Ϣ
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="tradeNo"></param>
        /// <param name="orderStatus"></param>
        /// <param name="confirmedTime"></param>
        /// <returns></returns>
        public int UpdateCreditOrderInfo(int orderId, string tradeNo, int orderStatus, string confirmedTime)
        {
            DbParameter[] parms = {
                                      DbHelper.MakeInParam("@orderid",(DbType)NpgsqlDbType.Integer,4,orderId),
                                      DbHelper.MakeInParam("@tradeno",(DbType)NpgsqlDbType.Char,32,tradeNo),
                                      DbHelper.MakeInParam("@orderstatus",(DbType)NpgsqlDbType.Smallint,2,orderStatus),
                                      DbHelper.MakeInParam("@confirmedtime",(DbType)NpgsqlDbType.Timestamp,4,confirmedTime)
                                  };
            return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("[{0}updateorderinfo]", BaseConfigs.GetTablePrefix), parms);

        }

        /// <summary>
        /// ����feed��Ϣ
        /// </summary>
        /// <param name="feedInfo"></param>
        /// <returns></returns>
        //public int PublishFeed(MiniFeedInfo feedInfo)
        //{
        //    DbParameter[] parms = {
        //                              DbHelper.MakeInParam("@uid",(DbType)NpgsqlDbType.Integer,4,feedInfo.Uid),
        //                              DbHelper.MakeInParam("@feedtype",(DbType)NpgsqlDbType.Smallint,4,(int)feedInfo.FeedType),
        //                              DbHelper.MakeInParam("@appid",(DbType)NpgsqlDbType.Integer,4,feedInfo.AppId),
        //                              DbHelper.MakeInParam("@username",(DbType)NpgsqlDbType.Char,20,feedInfo.UserName),
        //                              DbHelper.MakeInParam("@titletemplate",(DbType)NpgsqlDbType.Text,0,feedInfo.TitleTemplate),
        //                              DbHelper.MakeInParam("@titledata",(DbType)NpgsqlDbType.Text,0,feedInfo.TitleData),
        //                              DbHelper.MakeInParam("@bodytemplate",(DbType)NpgsqlDbType.Text,0,feedInfo.BodyTemplate),
        //                              DbHelper.MakeInParam("@bodydata",(DbType)NpgsqlDbType.Text,0,feedInfo.BodyData),
        //                              DbHelper.MakeInParam("@bodygeneral",(DbType)NpgsqlDbType.Text,0,feedInfo.BodyGeneral),
        //                              DbHelper.MakeInParam("@image1",(DbType)NpgsqlDbType.Varchar,255,feedInfo.Image1Url),
        //                              DbHelper.MakeInParam("@image1link",(DbType)NpgsqlDbType.Varchar,255,feedInfo.Image1Link),
        //                              DbHelper.MakeInParam("@image2",(DbType)NpgsqlDbType.Varchar,255,feedInfo.Image2Url),
        //                              DbHelper.MakeInParam("@image2link",(DbType)NpgsqlDbType.Varchar,255,feedInfo.Image2Link),
        //                              DbHelper.MakeInParam("@image3",(DbType)NpgsqlDbType.Varchar,255,feedInfo.Image3Url),
        //                              DbHelper.MakeInParam("@image3link",(DbType)NpgsqlDbType.Varchar,255,feedInfo.Image3Link),
        //                              DbHelper.MakeInParam("@image4",(DbType)NpgsqlDbType.Varchar,255,feedInfo.Image4Url),
        //                              DbHelper.MakeInParam("@image4link",(DbType)NpgsqlDbType.Varchar,255,feedInfo.Image4Link)
        //                          };
        //    return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, string.Format("[{0}publishfeed]", BaseConfigs.GetTablePrefix), parms);
        //}

        /// <summary>
        /// ��ȡ�û���feed
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        //public IDataReader GetUserFeeds(int uid, int pageIndex)
        //{
        //    DbParameter[] parms = {
        //                              DbHelper.MakeInParam("@uid", (DbType)NpgsqlDbType.Integer, 4, uid),
        //                              DbHelper.MakeInParam("@pageindex", (DbType)NpgsqlDbType.Integer, 4, pageIndex)
        //                          };
        //    return DbHelper.ExecuteReader(CommandType.StoredProcedure, string.Format("[{0}getuserfeeds]", BaseConfigs.GetTablePrefix), parms);
        //}

        /// <summary>
        /// ��ȡ����ͼ����Ϣ
        /// </summary>
        /// <param name="field">��ȡ�ֶ�</param>
        /// <param name="begin">��ʼ����</param>
        /// <param name="end">��������</param>
        /// <returns></returns>
        public IDataReader GetTrendGraph(string field, string begin, string end)
        {
            string sql = string.Format("SELECT {0} FROM [{1}trendstat] WHERE [daytime] >= {2} AND [daytime] <= {3}", field, BaseConfigs.GetTablePrefix, begin, end);
            return DbHelper.ExecuteReader(CommandType.Text, sql);
        }

        /// <summary>
        /// ����ָ����������displayorder��Ϣ
        /// </summary>
        /// <param name="displayorder">Ҫ���µ�displayorder��Ϣ</param>
        /// <param name="fid">���id</param>
        public void UpdateDisplayorderInForumByFid(int displayorder, int fid)
        {
            DbParameter[] prams =
			{
                DbHelper.MakeInParam("@displayorder", (DbType)NpgsqlDbType.Integer, 4, displayorder),
                DbHelper.MakeInParam("@fid", (DbType)NpgsqlDbType.Integer, 4, fid)
			};
            string sql = "UPDATE [" + BaseConfigs.GetTablePrefix + "forums] SET [displayorder]=@displayorder WHERE [fid]=@fid";
            DbHelper.ExecuteDataset(CommandType.Text, sql, prams);
        }

        /// <summary>
        /// �������ð��ģ����Ϣ
        /// </summary>
        /// <param name="templateID">�µ�ģ��id</param>
        /// <param name="fidlist">Ҫ���µİ��id�б�</param>
        /// <returns></returns>
        public int UpdateForumTemplateID(int templateID, string fidlist)
        {
            DbParameter[] prams =
			{
                DbHelper.MakeInParam("@templateid", (DbType)NpgsqlDbType.Integer, 4, templateID),
         	};
            string sql = "UPDATE [" + BaseConfigs.GetTablePrefix + "forums] SET [templateid]=@templateid WHERE [fid] IN (" + fidlist + ")";
            return DbHelper.ExecuteNonQuery(CommandType.Text, sql, prams);
        }
    }
}
