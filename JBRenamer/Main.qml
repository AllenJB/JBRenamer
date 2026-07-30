import QtQuick
import QtQuick.Controls
import QtQuick.Layouts
import QtQuick.Dialogs

ApplicationWindow {
    id: mainWindow
    visible: true
    title: "JBRenamer"
    width: 800
    height: 600
    
    Program {
        id: program
    }

    FilesModel {
        id: files
    }

    RulesModel {
        id: rules
    }
    
    DebugModel {
        id: debug
    }
    
    signal fileListUpdated()
    signal ruleListUpdated()
    
    onFileListUpdated: function() {
        files.runRules(rules);
        Qt.callLater(function() {
            fileTable.forceLayout();
        })
    }
    
    onRuleListUpdated: function() {
        files.runRules(rules);
        Qt.callLater(function() {
            fileTable.forceLayout();
            rulesTable.forceLayout();
        })
    }

    FileDialog {
        id: addSourceFileDialog
        acceptLabel: "Add Source File(s)"
        fileMode: FileDialog.OpenFiles
        options: FileDialog.DontResolveSymlinks | FileDialog.HideNameFilterDetails
        onAccepted: {
            files.addSourceFile(selectedFile);
            mainWindow.fileListUpdated();
        }
    }

    FolderDialog {
        id: addSourceDirDialog
        acceptLabel: "Add Source Files From Directory"
        options: FolderDialog.DontResolveSymlinks
        onAccepted: {
            files.addSourceDirectory(selectedFolder);
            mainWindow.fileListUpdated();
        }
    }

    menuBar: MenuBar {
        Menu {
            title: qsTr("&File")

            Action {
                text: "Add Source File(s)"
                onTriggered: {
                    addSourceFileDialog.open()
                }
            }
            Action {
                text: "Add Source Files From Directory"
                onTriggered: {
                    addSourceDirDialog.open()
                }
            }
            Action {
                text: "Rename files"
                onTriggered: {
                    files.renameFiles(rules);
                    fileListUpdated();
                }
            }
            Action {
                text: "Clear File List"
                onTriggered: {
                    files.clear();
                    fileListUpdated();
                }
            }
            Action {
                text: qsTr("&Quit")
                onTriggered: mainWindow.close()
            }
        }
        
        Menu {
            title: "Rules"
            
            Action {
                text: "Add Rule"
                onTriggered: {
                    ruleAddDialog.open()
                }
            }
        }
    }
    
    RuleAddDialog {
        id: ruleAddDialog
    }
    
    SplitView {
        anchors.fill: parent
        orientation: Qt.Vertical

        ColumnLayout {
            Layout.fillWidth: true
            Layout.fillHeight: true
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
                model: rules
                Layout.fillWidth: true
                Layout.fillHeight: true

                alternatingRows: true
                columnSpacing: 2
                delegate: TableViewDelegate {
                    implicitWidth: 1 * mainWindow.width / 3
                    padding: 5
                }

                property var columnWidths: [50, 100, rulesTableContainer.width]
                columnWidthProvider: function(column) {
                    let w = explicitColumnWidth(column)
                    if (w >= 0 && column !== (columns - 1)) {
                        columnWidths[column] = w;
                        return w;
                    }
                    if (column === (columns - 1)) {
                        w =  columnWidths[column];
                        let i = columns - 1;
                        while(--i !== -1) w -= columnWidths[i];
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


            DropArea {
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
                        delegate: TableViewDelegate {
                            implicitWidth: 9 * mainWindow.width / 20
                            padding: 5
                        }

                        property var columnWidths: [300, 300, 75, fileTableContainer.width]
                        columnWidthProvider: function(column) {
                            let w = explicitColumnWidth(column)
                            if (w >= 0 && column !== (columns - 1)) {
                                columnWidths[column] = w;
                                return w;
                            }
                            if (column === (columns - 1)) {
                                w =  columnWidths[column];
                                let i = columns - 1;
                                while(--i !== -1) w -= columnWidths[i];
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
            }
        }
    }
}
