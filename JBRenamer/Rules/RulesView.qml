import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

ColumnLayout {
    id: rulesTableContainer

    HorizontalHeaderView {
        syncView: rulesTable
        Layout.row: 1
        Layout.column: 1
        Layout.fillWidth: true
        delegate: HorizontalHeaderViewDelegate {
            padding: 5
        }
    }
    TableView {
        id: rulesTable
        model: program.rules
        Layout.fillWidth: true
        Layout.fillHeight: true

        alternatingRows: true
        columnSpacing: 2
        delegate: TableViewDelegate {
            implicitWidth: 1 * mainWindow.width / 3
            padding: 5
        }

        property var columnWidths: [50, 100, rulesTableContainer.width]
        columnWidthProvider: function (column) {
            let w = explicitColumnWidth(column)
            if (w >= 0 && column !== (columns - 1)) {
                columnWidths[column] = w;
                return w;
            }
            if (column === (columns - 1)) {
                w = columnWidths[column];
                let i = columns - 1;
                while (--i !== -1) w -= columnWidths[i];
                return w;
            }
            return columnWidths[column];
        }

        selectionBehavior: TableView.SelectRows
        selectionMode: TableView.ExtendedSelection
        editTriggers: TableView.NoEditTriggers
        ScrollBar.vertical: ScrollBar {
            policy: ScrollBar.AlwaysOn
        }
    }
}
