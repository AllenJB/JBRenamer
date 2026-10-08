import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs
// The ../ is required here because that's how the files end up laid out in the bin/Debug/net10.0/ directory
import "../Presets" as Presets

ApplicationWindow {
    id: mainWindow
    visible: true
    title: "JBRenamer"
    width: 800
    height: 600

    Program {
        id: program

        signal fileListUpdated()
        signal ruleListUpdated()
        signal filtersUpdated()

        onShowError: function (args) {
            popupError.message = args.message;
            popupError.open();
        }

        onFileListUpdated: function () {
            program.files.runRules(program.rules);
        }

        onRuleListUpdated: function() {
            program.files.runRules(program.rules);
            Qt.callLater(function () {
                rulesTable.forceLayout();
            });
        }
    }

    DebugModel {
        id: debug
    }

    Dialog {
        id: popupError
        property string message
        title: "Error!"
        anchors.centerIn: parent
        modal: true
        focus: true
        closePolicy: Popup.CloseOnEscape
        standardButtons: Dialog.Ok
        Text {
            Layout.alignment: Qt.AlignCenter
            text: popupError.message
        }
    }

    FileDialog {
        id: addSourceFileDialog
        acceptLabel: "Add Source File(s)"
        fileMode: FileDialog.OpenFiles
        options: FileDialog.DontResolveSymlinks | FileDialog.HideNameFilterDetails
        onAccepted: {
            try {
                program.files.addSourceFile(selectedFile);
            } catch (e) {
                console.log("Xception:", e);
                Debug.log("Exception: " + e);
            }
            program.fileListUpdated();
        }
    }

    FolderDialog {
        id: addSourceDirDialog
        acceptLabel: "Add Source Files From Directory"
        options: FolderDialog.DontResolveSymlinks
        onAccepted: {
            program.files.addSourceDirectory(selectedFolder);
            program.fileListUpdated();
        }
    }

    FiltersDialog {
        id: filtersDialog
    }

    Presets.AddDialog {
        id: presetAddDialog
    }

    menuBar: MenuBar {
        Menu {
            title: qsTr("&File")

            Action {
                text: "Configure file filters"
                onTriggered: {
                    filtersDialog.open()
                }
            }
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
                    program.files.renameFiles(program.rules);
                    program.fileListUpdated();
                }
            }
            Action {
                text: "Clear File List"
                onTriggered: {
                    program.files.clear();
                    program.fileListUpdated();
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
            Action {
                text: "Clear Rule List"
                onTriggered: {
                    program.rules.clear();
                    program.ruleListUpdated();
                }
            }
        }

        Presets.Menu {}
    }

    footer: StatusBar {
        id: statusBar
    }

    RuleAddDialog {
        id: ruleAddDialog
    }

    SplitView {
        id: listsContainer
        anchors.fill: parent
        orientation: Qt.Vertical

        ColumnLayout {
            id: rulesTableContainer

            Layout.fillWidth: true
            SplitView.preferredHeight: (listsContainer.height / 2)
            SplitView.minimumHeight: 100

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

        FilesView {
            id: filesView

            Layout.fillWidth: true
            SplitView.preferredHeight: (listsContainer.height / 2)
            SplitView.minimumHeight: 100
        }
    }
}
