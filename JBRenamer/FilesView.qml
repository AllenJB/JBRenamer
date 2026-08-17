import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

DropArea {
    signal fileListUpdated()
    signal ruleListUpdated()
    
    onFileListUpdated: function() {
        Qt.callLater(function() {
            fileTable.forceLayout();
        });

        dropFilesLabel.visible = (files.count() == 0);
    }
    
    onRuleListUpdated: function() {
        Qt.callLater(function() {
            fileTable.forceLayout();
        });
    }
    
    Layout.fillWidth: true
    Layout.fillHeight: true

    onDropped: function(drop) {
        // Both DragEvent text and urls properties appear to be seperated lists
        // with text using newline, while urls uses comma
        // There appears to be no way to tell when a comma appears in a filename, while \n is much less common
        // So .text is used
        files.drop(drop.formats, drop.text);
        mainWindow.fileListUpdated();
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
            model: files
            Layout.fillWidth: true
            Layout.fillHeight: true

            alternatingRows: true
            columnSpacing: 2
            delegate: DelegateChooser {
                DelegateChoice {
                    column: files.columnIndex("New Name")
                    delegate: TableViewDelegate {
                        id: newNameDelegate
                        contentItem: Label {
                            padding: 5
                            color: {
                                // Trick the engine into making this link to displayed value changes
                                console.log(newNameDelegate.model.display);

                                if (files.destinationConflicts(newNameDelegate.row)) {
                                    return "red";
                                }
                                if (files.destinationChanged(newNameDelegate.row)) {
                                    return "darkblue";
                                }

                                return "darkgray";
                            }
                            text: (newNameDelegate.model.display ?? "")
                        }
                    }
                }
                DelegateChoice {
                    delegate: TableViewDelegate {
                        padding: 5
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
                let columnName = files.columnName(column);

                let w = explicitColumnWidth(column);
                if (w >= 0) {
                    columnWidths[columnName] = w;
                }
                // debug.log("Column width for " + columnName + " = " + columnWidths[columnName]);
                return columnWidths[columnName];
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
        }
    }
    Text {
        id: dropFilesLabel
        anchors.centerIn: parent
        horizontalAlignment: Text.AlignHCenter
        verticalAlignment: Text.AlignVCenter
        text: "Drag your files here"
        color: "#000066"
        font.pixelSize: 24
        font.weight: Font.DemiBold
    }
}
