namespace UtilityLibrary.Excel
{
    internal static class ExcelMapper
    {
        internal static TEntity MapDtoToEntity<TDto, TEntity>(TDto dto)
            where TEntity : new()
        {
            var entity = new TEntity();
            var dtoProps = typeof(TDto).GetProperties();
            var entityProps = typeof(TEntity).GetProperties();

            foreach (var eProp in entityProps)
            {
                var dProp = dtoProps.FirstOrDefault(p =>
                    p.Name.Equals(eProp.Name, StringComparison.OrdinalIgnoreCase));

                if (dProp != null)
                {
                    var val = dProp.GetValue(dto);
                    if (val != null && eProp.PropertyType != dProp.PropertyType)
                    {
                        try
                        {
                            var targetType = Nullable.GetUnderlyingType(eProp.PropertyType) ?? eProp.PropertyType;
                            val = Convert.ChangeType(val, targetType);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    eProp.SetValue(entity, val);
                }
            }
            return entity;
        }
    }
}
