using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Data
{
    public class DataPersistSize
    {
        public readonly Func<IData, int> getSize;

        public readonly Type Type;

        public readonly Func<Type, MemberInfo, int> MembersOrder;
        public readonly AllowNull AllowNull;
        public readonly int CharSize;

        public DataPersistSize(Type type, AllowNull allowNull = AllowNull.None, Func<Type, MemberInfo, int> membersOrder = null)
            : this(type, 1, allowNull, membersOrder)
        {
        }

        public DataPersistSize(Type type, int charSize, AllowNull allowNull = AllowNull.None, Func<Type, MemberInfo, int> membersOrder = null)
        {
            Type = type;

            CharSize = charSize;
            AllowNull = allowNull;
            MembersOrder = membersOrder;

            getSize = CreateGetSizeMethod().Compile();
        }

        public int GetSize(IData data)
        {
            return getSize(data);
        }

        public Expression<Func<IData, int>> CreateGetSizeMethod()
        {
            var idata = Expression.Parameter(typeof(IData), "idata");

            var dataType = typeof(Data<>).MakeGenericType(Type);
            var dataValue = Expression.Variable(Type, "dataValue");

            var assign = Expression.Assign(dataValue, Expression.Convert(idata, dataType).Value());

            var body = PersistSizeHelper.CreateSizeBody(dataValue, CharSize, MembersOrder, AllowNull);

            return Expression.Lambda<Func<IData, int>>(Expression.Block(new ParameterExpression[] { dataValue }, assign, body), idata);
        }
    }
}