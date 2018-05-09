custom_log 'custom_log' do msg 'Cleaning up the VM by deleting install files' end
start_time = DateTime.now
custom_log 'custom_log' do msg "recipe_start_time(#{recipe_name}): #{start_time}" end

# Remove STMP instance settings which were set for the smoke tests failure email
include_recipe 'Relativity::post_relativity_instance_setting_update_smtp_email_settings_cleanup'

# Delete install files
powershell_script 'Delete install files' do
  code <<-EOH
    $fso = New-Object -ComObject scripting.filesystemobject
    $fso.DeleteFolder("C:\\Chef_Install")

    exit 0
  EOH
end

# Delete vagrant properties which contains workstation domain account credentails
powershell_script 'Delete vagrant properties' do
  code <<-EOH
    [string] $dna_json_file = "C:\\vagrant-chef\\dna.json"
    $dna_json_file_content = Get-Content -Path $dna_json_file | ConvertFrom-Json
    $dna_json_file_content.smb_domain = "abcde"
    $dna_json_file_content.smb_username = "abcde"
    $dna_json_file_content.smb_password = "abcde"
    $dna_json_file_content.smtp_server = "abcde"
    $dna_json_file_content.smtp_port = "abcde"
    $dna_json_file_content.email_from = "abcde"
    $dna_json_file_content.email_to = "abcde"
    $dna_json_file_content | ConvertTo-Json  | Set-Content $dna_json_file

    exit 0
  EOH
end

end_time = DateTime.now
custom_log 'custom_log' do msg "recipe_end_Time(#{recipe_name}): #{end_time}" end
custom_log 'custom_log' do msg "recipe_duration(#{recipe_name}): #{end_time.to_time - start_time.to_time} seconds" end
custom_log 'custom_log' do msg "Cleaned up the VM by deleting install files\n\n\n" end