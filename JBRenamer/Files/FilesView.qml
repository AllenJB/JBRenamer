import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

DropArea {
    Connections {
        target: program

        function onFileListUpdated() {
            Qt.callLater(function () {
                fileTable.forceLayout();
            });

            dropFilesLabel.visible = (program.files.count() === 0);
        }

        function onRuleListUpdated() {
            Qt.callLater(function () {
                fileTable.forceLayout();
            });
        }
    }

    Layout.fillWidth: true
    Layout.fillHeight: true

    onDropped: function (drop) {
        // Both DragEvent text and urls properties appear to be seperated lists
        // with text using newline, while urls uses comma
        // There appears to be no way to tell when a comma appears in a filename, while \n is much less common
        // So .text is used
        program.files.drop(drop.text, program.filters);
        program.fileListUpdated();
    }

    ColumnLayout {
        anchors.fill: parent
        id: fileTableContainer

        HorizontalHeaderView {
            syncView: fileTable
            Layout.row: 1
            Layout.column: 1
            Layout.fillWidth: true
            delegate: HorizontalHeaderViewDelegate {
                padding: 5
            }
        }
        TableView {
            id: fileTable
            model: program.files
            Layout.fillWidth: true
            Layout.fillHeight: true

            alternatingRows: true
            columnSpacing: 2
            delegate: DelegateChooser {
                DelegateChoice {
                    column: program.files.columnIndex("New Name")
                    delegate: TableViewDelegate {
                        id: newNameDelegate
                        contentItem: Label {
                            padding: 5
                            color: {
                                // Trick the engine into making this link to displayed value changes
                                console.log(newNameDelegate.model.display);

                                if (program.files.destinationConflicts(newNameDelegate.row)) {
                                    return (Application.styleHints.colorScheme === Qt.ColorScheme.Light ? "red" : "tomato");
                                }
                                if (program.files.destinationChanged(newNameDelegate.row)) {
                                    return palette.accent;
                                }

                                return palette.placeholderText;
                            }
                            text: (newNameDelegate.model.display ?? "")

                            MouseArea {
                                anchors.fill: parent

                                onClicked: fileSelection.clickRow(mouse, row)
                            }
                        }

                        ContextMenu.menu: rowContextMenu
                    }
                }
                DelegateChoice {
                    delegate: TableViewDelegate {
                        padding: 5
                        id: tvd

                        ContextMenu.menu: rowContextMenu
                        contentItem: Label {
                            clip: false
                            text: tvd.model.display ?? ""
                            elide: Text.ElideRight
                            color: tvd.highlighted ? tvd.palette.highlightedText : tvd.palette.buttonText
                            visible: !tvd.editing

                            // TODO Clicking the edge / between rows seems to trigger something different
                            MouseArea {
                                anchors.fill: parent

                                onClicked: fileSelection.clickRow(mouse, row)
                            }
                        }
                    }
                }
            }

            property var columnWidths: {
                "Original Full Path": 500,
                "Original Name": 250,
                "New Name": 250,
                "New Full Path": 500,
                "Status": 75,
                "Error Message": 250,
            }
            columnWidthProvider: function (column) {
                let columnName = program.files.columnName(column);

                let w = explicitColumnWidth(column);
                if (w >= 0) {
                    columnWidths[columnName] = w;
                }
                // debug.log("Column width for " + columnName + " = " + columnWidths[columnName]);
                return columnWidths[columnName];
            }

            acceptedButtons: Qt.NoButton
            selectionModel: ItemSelectionModel {
                id: fileSelection
                property int prevRow: -1

                onSelectionChanged: function () {
                    console.log("selectedIndexes count: " + selectedIndexes.length);
                    console.log(selectedIndexes)
                }
                onCurrentChanged: function () {
                    console.log("selection current changed:", currentIndex);
                }

                signal clickRow(mouse: MouseEvent, row: int);

                onClickRow: function (mouse, row) {
                    let prevRow = fileSelection.prevRow
                    console.log("click", mouse, mouse.button, mouse.modifiers);
                    if (mouse.button !== Qt.LeftButton) {
                        console.log("Not left button");
                        return;
                    }
                    if (mouse.modifiers === Qt.NoModifier) {
                        console.log("No modifier");
                        fileSelection.select(fileTable.model.index(row, 0), ItemSelectionModel.ClearAndSelect | ItemSelectionModel.Rows);
                    } else if (mouse.modifiers & Qt.ShiftModifier) {
                        console.log("Shift modifier :: prevRow = " + prevRow);
                        if ((prevRow === -1) || (prevRow === row)) {
                            fileSelection.select(fileTable.model.index(row, 0), ItemSelectionModel.Select | ItemSelectionModel.Rows);
                        } else {
                            let firstRow = (prevRow < row ? prevRow : row);
                            let lastRow = (prevRow < row ? row : prevRow);
                            for (let iRow = firstRow; iRow <= lastRow; iRow++) {
                                console.log("Add row to selection: " + iRow);
                                fileSelection.select(fileTable.model.index(iRow, 0), ItemSelectionModel.Select | ItemSelectionModel.Rows);
                            }
                        }
                    } else if (mouse.modifiers & Qt.ControlModifier) {
                        console.log("Ctrl modifier");
                        fileSelection.select(fileTable.model.index(row, 0), ItemSelectionModel.Toggle | ItemSelectionModel.Rows);
                    }

                    fileSelection.prevRow = row;
                    console.log("prevRow updated to " + fileSelection.prevRow);
                }
            }
            selectionBehavior: TableView.SelectRows
            selectionMode: TableView.ExtendedSelection
            editTriggers: TableView.NoEditTriggers
            ScrollBar.vertical: ScrollBar {
                policy: ScrollBar.AlwaysOn
            }
            ScrollBar.horizontal: ScrollBar {
                policy: ScrollBar.AlwaysOn
            }

            ContextMenu.menu: Menu {
                MenuItem {
                    text: "Clear file list"
                    onTriggered: {
                        program.files.clear();
                        program.fileListUpdated();
                    }
                }
            }

            Menu {
                id: rowContextMenu
                // @formatter:off
                readonly property TableViewDelegate triggerItem: parent as TableViewDelegate
                // @formatter:on

                MenuItem {
                    text: "X Clear file list"
                    onTriggered: {
                        program.files.clear();
                        program.fileListUpdated();
                    }
                }
                MenuItem {
                    text: "Row: " + rowContextMenu.triggerItem.row + ", " + rowContextMenu.triggerItem.column + " = " + rowContextMenu.triggerItem.model.display
                    onTriggered: {
                        console.log(rowContextMenu.triggerItem)
                    }
                }
                MenuItem {
                    enabled: fileSelection.hasSelection
                    text: "Remove selected items"
                    onTriggered: {
                        let selectedIndexes = fileSelection.selectedIndexes;
                        console.log("Remove items: " + selectedIndexes);
                        
                        let selectedRows = [];
                        for (let idx of selectedIndexes) {
                            if (selectedRows.includes(idx.row)) {
                                continue;
                            }
                            selectedRows.push(idx.row);
                        }
                        fileSelection.clear();
                        
                        // Process rows in reverse order to avoid index shifts
                        selectedRows.sort().reverse();
                        console.log(selectedRows);
                        for (let row of selectedRows) {
                            program.files.removeRow(row);
                        }

                        program.fileListUpdated();
                    }
                }
            }
        }
    }
    Text {
        id: dropFilesLabel
        anchors.centerIn: parent
        horizontalAlignment: Text.AlignHCenter
        verticalAlignment: Text.AlignVCenter
        text: "Drag your files here"
        color: palette.placeholderText
        font.pixelSize: 24
        font.weight: Font.DemiBold
    }
}
