custom_log 'custom_log' do msg 'Starting software install' end
start_time = DateTime.now
custom_log 'custom_log' do msg "recipe_start_time(#{recipe_name}): #{start_time}" end

# Install Software
include_recipe 'chocolatey'

custom_log 'custom_log' do msg 'Finished installing chocolatey' end

# Install Notepad++
chocolatey_package 'notepadplusplus' do
  version '7.4.2'
  retries 5
  retry_delay 5
  action :install
end

custom_log 'custom_log' do msg 'Finished installing notepad++' end

# Install Visual Studio 2015 Remote Debugger
chocolatey_package 'vs2015remotetools' do
  version '14.0.25424.0'
  retries 5
  retry_delay 5
  action :install
end

custom_log 'custom_log' do msg 'Finished installing Visual Studio 2015 Remote Debugger' end

# Install Visual Studio 2017 Remote Debugger
chocolatey_package 'visualstudio2017-remotetools' do
  version '15.0.26430.2'
  retries 5
  retry_delay 5
  action :install
end

custom_log 'custom_log' do msg 'Finished installing Visual Studio 2017 Remote Debugger' end

# Install 7 zip software
node.default['seven_zip']['syspath'] = true
node.default['seven_zip']['home'] = "c:\\7-zip"
include_recipe 'seven_zip::default'

custom_log 'custom_log' do msg 'Finished installing 7zip' end

# Install Adobe Reader
chocolatey_package 'adobereader' do
  version '2015.007.20033.02'
  retries 5
  retry_delay 5
  action :install
end

custom_log 'custom_log' do msg 'Finished installing Adobe Reader' end

# Install JungUm
include_recipe 'Relativity::windows_install_jungum'

custom_log 'custom_log' do msg 'Finished installing JungUm' end

# Install Lotus Notes
include_recipe 'Relativity::windows_install_lotus_notes'

custom_log 'custom_log' do msg 'Finished installing Lotus Notes' end

# Install Microsoft Office
include_recipe 'Relativity::windows_install_msoffice'

custom_log 'custom_log' do msg 'Finished installing Microsoft Office' end

# Install Microsoft Works Converter
include_recipe 'Relativity::windows_install_msworksconverter'

custom_log 'custom_log' do msg 'Finished installing Microsoft Works Converter' end

end_time = DateTime.now
custom_log 'custom_log' do msg "recipe_end_Time(#{recipe_name}): #{end_time}" end
custom_log 'custom_log' do msg "recipe_duration(#{recipe_name}): #{end_time.to_time - start_time.to_time} seconds" end
custom_log 'custom_log' do msg "Finished software install\n\n\n" end
