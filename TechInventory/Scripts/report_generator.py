import sqlite3
import sys
import os
import pandas as pd

def generate_report(report_type, db_path, output_dir):
    if not os.path.exists(db_path):
        print(f"Ошибка: база данных не найдена по пути {db_path}")
        return

    conn = sqlite3.connect(db_path)
    try:
        if report_type == 'purchase':
            generate_purchase_report(conn, output_dir)
        elif report_type == 'inventory':
            generate_inventory_report(conn, output_dir)
        elif report_type == 'tickets':
            generate_tickets_report(conn, output_dir)
        else:
            print(f"Неизвестный тип отчёта: {report_type}")
    finally:
        conn.close()

def generate_purchase_report(conn, output_dir):
    query = """
        SELECT d.Name, d.Specs, d.StatusID, dt.Value as DeviceType,
               r.Name as RoomName, u.FullName as AssignedTo
        FROM Devices d
        LEFT JOIN Dictionary dt ON d.TypeID = dt.ID AND dt.Category = 'DeviceType'
        LEFT JOIN Rooms r ON d.CurrentRoomID = r.RoomID
        LEFT JOIN Users u ON d.AssignedToUserID = u.UserID
    """
    df = pd.read_sql_query(query, conn)

    status_query = "SELECT ID, Value FROM Dictionary WHERE Category = 'DeviceStatus'"
    status_df = pd.read_sql_query(status_query, conn)
    status_map = dict(zip(status_df['ID'], status_df['Value']))
    df['StatusName'] = df['StatusID'].map(status_map)

    broken_id = 4
    broken_df = df[df['StatusID'] == broken_id]

    if broken_df.empty:
        with pd.ExcelWriter(os.path.join(output_dir, 'purchase_report.xlsx')) as writer:
            pd.DataFrame({'Сообщение': ['Нет сломанных устройств']}).to_excel(writer, index=False)
        print("Отчёт для закупки создан. Сломанных устройств нет.")
        return

    grouped = broken_df.groupby(['DeviceType', 'StatusName']).agg(
        Количество=('Name', 'count'),
        Устройства=('Name', lambda x: ', '.join(x))
    ).reset_index()

    total_by_type = df.groupby('DeviceType').size().reset_index(name='TotalCount')
    grouped = grouped.merge(total_by_type, on='DeviceType', how='left')
    grouped['Процент сломанных'] = (grouped['Количество'] / grouped['TotalCount'] * 100).round(1).astype(str) + '%'

    output_path = os.path.join(output_dir, 'purchase_report.xlsx')
    with pd.ExcelWriter(output_path) as writer:
        grouped.to_excel(writer, sheet_name='Сломанное оборудование', index=False)
        broken_df[['Name', 'DeviceType', 'RoomName', 'AssignedTo', 'StatusName']].to_excel(
            writer, sheet_name='Детальный список', index=False
        )
    print(f"Отчёт для закупки сохранён: {output_path}")

def generate_inventory_report(conn, output_dir):
    query = """
        SELECT d.Name, dt.Value as DeviceType, ds.Value as StatusName,
               r.Name as RoomName, u.FullName as AssignedTo, d.PositionInRoom, d.Specs
        FROM Devices d
        LEFT JOIN Dictionary dt ON d.TypeID = dt.ID AND dt.Category = 'DeviceType'
        LEFT JOIN Dictionary ds ON d.StatusID = ds.ID AND ds.Category = 'DeviceStatus'
        LEFT JOIN Rooms r ON d.CurrentRoomID = r.RoomID
        LEFT JOIN Users u ON d.AssignedToUserID = u.UserID
        ORDER BY r.Name, d.PositionInRoom
    """
    df = pd.read_sql_query(query, conn)
    output_path = os.path.join(output_dir, 'inventory_report.xlsx')
    with pd.ExcelWriter(output_path) as writer:
        df.to_excel(writer, sheet_name='Инвентаризация', index=False)
    print(f"Инвентаризационный отчёт сохранён: {output_path}")

def generate_tickets_report(conn, output_dir):
    # В твоей таблице Tickets нет столбца ClosedAt, поэтому мы его не используем
    query = """
        SELECT t.TicketID, d.Name as DeviceName, t.Description,
               CASE t.Priority
                   WHEN 1 THEN 'Высокий'
                   WHEN 2 THEN 'Средний'
                   ELSE 'Низкий'
               END as Priority,
               CASE t.StatusID
                   WHEN 1 THEN 'Новая'
                   WHEN 2 THEN 'В работе'
                   WHEN 3 THEN 'Завершена'
                   ELSE 'Неизвестно'
               END as Status,
               t.CreatedAt
        FROM Tickets t
        JOIN Devices d ON t.DeviceID = d.DeviceID
        ORDER BY t.CreatedAt DESC
    """
    df = pd.read_sql_query(query, conn)
    df['CreatedAt'] = pd.to_datetime(df['CreatedAt'])

    stats = {
        'Всего заявок': len(df),
        'Открыто': len(df[df['Status'].isin(['Новая', 'В работе'])]),
        'Закрыто': len(df[df['Status'] == 'Завершена']),
        'По приоритетам': df['Priority'].value_counts().to_dict()
    }

    output_path = os.path.join(output_dir, 'tickets_report.xlsx')
    with pd.ExcelWriter(output_path) as writer:
        df.to_excel(writer, sheet_name='Заявки', index=False)
        stats_df = pd.DataFrame({
            'Показатель': stats.keys(),
            'Значение': [str(v) for v in stats.values()]
        })
        stats_df.to_excel(writer, sheet_name='Статистика', index=False)
    print(f"Отчёт по заявкам сохранён: {output_path}")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Использование: python report_generator.py <тип_отчёта> <путь_к_БД> [выходная_папка]")
        sys.exit(1)

    report_type = sys.argv[1]
    db_path = sys.argv[2]
    output_dir = sys.argv[3] if len(sys.argv) > 3 else os.path.dirname(db_path)
    generate_report(report_type, db_path, output_dir)